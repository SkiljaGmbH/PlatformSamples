using EDAClient.Common;
using EDAClient.Data;
using EDAClient.Views;
using MaterialDesignThemes.Wpf;
using SampleEventDrivenActivity.Configuration;
using STG.Common.DTO;
using STG.Common.DTO.EventDriven;
using STG.Common.DTO.Search;
using STG.Common.Utilities.Exceptions;
using STG.RT.API.Signaling;
using STG.RT.API.Signaling.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EDAClient.ViewModels
{
    public class RuntimeViewModel : INotifyPropertyChanged
    {
        private ICommand _fetchCommand;
        private IEventDrivenNotificationClient _signalingClient;
        private ICommand _uploadCommand;
        private readonly List<int> _ourWorkItems = new List<int>();


        public RuntimeViewModel()
        {
            Clear();
        }

        public DtoActivityInstanceInfo EDAInitializer { get; set; }

        public DtoActivityInstanceInfo EDAResponder { get; set; }

        public bool CanUpload { get; set; }


        public bool CanDownload { get; set; }

        public bool IsUploading { get; set; }

        public ICommand UploadCommand => _uploadCommand ??
                                         (_uploadCommand = new AsyncCommandExecutor(UploadCommand_execute, uploadCommand_canExecute, nameof(UploadCommand)));


        public ICommand FetchCommand => _fetchCommand ??
                                        (_fetchCommand = new AsyncCommandExecutor(fetchCommand_execute,
                                            fetchCommand_canExecute, nameof(FetchCommand)));

        public event PropertyChangedEventHandler PropertyChanged;

        private void Clear()
        {
            CanUpload = false;
            CanDownload = false;
            EDAInitializer = null;
            EDAResponder = null;
        }

        private async Task UploadCommand_execute()
        {
            var filePath = SharedData.IOService.OpenFileDialog(@"c:\");
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                IsUploading = true;
                var zip = PrepareUploadStream(filePath);

                var notification = await SharedData.EDA.SendToActivityAsync(EDAInitializer, zip);
                _ourWorkItems.Add(notification.WorkItemID);
                IsUploading = false;
            }
        }

        private Stream PrepareUploadStream(string filePath)
        {
            var uploadStream = new MemoryStream();
            var fi = new FileInfo(filePath);

            //Zips should be only added to system by skipping compressions
            if (fi.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                using (var fs = File.OpenRead(filePath))
                {
                    fs.CopyTo(uploadStream);
                }
            }
            else
            {
                using (var archive = new ZipArchive(uploadStream, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry(fi.Name);
                    using (var entryStream = entry.Open())
                    {
                        using (var fs = File.OpenRead(filePath))
                        {
                            fs.CopyTo(entryStream);
                        }
                    }

                    var metaDataPath = SharedData.IOService.GetDataFilePath(EDAInitializer.ActivityInstanceID);
                    fi = new FileInfo(metaDataPath);
                    entry = archive.CreateEntry("MetaData.json");
                    using (var entryStream = entry.Open())
                    {
                        using (var fs = File.OpenRead(metaDataPath))
                        {
                            fs.CopyTo(entryStream);
                        }
                    }
                }
            }
            uploadStream.Position = 0;
            return uploadStream;
        }

        private bool uploadCommand_canExecute()
        {
            return CanUpload;
        }

        private async Task fetchCommand_execute()
        {
            var allNotifications = await SharedData.EDA.GetPendingNotificationsAsync(new DtoActivitySelector { ActivityInstanceIdentifier = EDAResponder.Identifier });
            var extendedNotifications = ExtendNotifications(allNotifications.Where(x => x.Status == DtoNotificationStatus.Ready));

            if (extendedNotifications.Count == 0)
            {
                SharedData.SnackBarMessageQ.Enqueue(
                    $"There is no notifications available for the activity {EDAResponder.ActivityInstanceName}");
                return;
            }

            DtoEventDrivenNotification notification = extendedNotifications.First();
            if (extendedNotifications.Count > 1)
                notification = await SelectNotification(extendedNotifications);
            if (notification != null)
            {
                DtoEventDrivenNotification locked = await LockNotification(notification);
                if (locked == null)
                    return;
                var output = await DownloadNotification(locked);
                if (!string.IsNullOrWhiteSpace(output))
                {
                    await PreviewResult(output);
                }

                await AcknowledgeNotificationAsync(locked);
            }
            else
            {
                SharedData.SnackBarMessageQ.Enqueue($"No notification to retrieve");
            }
        }

        private static Task AcknowledgeNotificationAsync(DtoEventDrivenNotification notification)
        {
            return SharedData.EDA.DeleteNotificationAsync(notification);
        }

        private List<ExtendedNotification> ExtendNotifications(IEnumerable<DtoEventDrivenNotification> allNotifications)
        {
            return allNotifications.Select(x =>
            {
                var extendedNotification = new ExtendedNotification(x);
                extendedNotification.OurNotification = _ourWorkItems.Contains(x.WorkItemID);
                return extendedNotification;
            }).ToList();
        }

        private async Task<DtoEventDrivenNotification> LockNotification(DtoEventDrivenNotification notification)
        {
            if (notification.Status == DtoNotificationStatus.Locked)
                SharedData.SnackBarMessageQ.Enqueue("The notification you selected was already locked - you might be stealing it from someone else.");
            try
            {
                return await SharedData.EDA.LockNotificationAsync(notification);
            }
            catch (STGConcurrencyException ex)
            {
                SharedData.SnackBarMessageQ.Enqueue("The selected notification was already locked or taken by someone else - " + ex.Message);
                return null;
            }
        }

        private async Task<DtoEventDrivenNotification> SelectNotification(IList<ExtendedNotification> allNotifications)
        {
            var view = new SelectNotificationView
            {
                DataContext = new SelectNotificationViewModel(allNotifications)
            };

            var selection = await DialogHost.Show(view, "RootDialog", (s, e) => { }, (s, e) => { });
            return selection as DtoEventDrivenNotification;
        }


        /// <summary>
        /// Downloads the corresponding stream of a notification (must be locked) and then acknowledges the reception
        /// of the notification (aka: delete).
        /// </summary>
        private async Task<string> DownloadNotification(DtoEventDrivenNotification notification)
        {
            using (var ms = new MemoryStream())
            {
                var stream = await SharedData.EDA.GetEventStreamAsync(notification.RelatedStream);
                stream.CopyTo(ms);
                ms.Position = 0;

                var fi = new FileInfo(SharedData.IOService.GetDataFilePath(EDAResponder.ActivityInstanceID));
                var resultPath = Path.Combine(fi.DirectoryName, "Results", EDAResponder.Identifier.ToString(),
                    notification.WorkItemID.ToString());
                if (Directory.Exists(resultPath)) Directory.Delete(resultPath, true);
                Directory.CreateDirectory(resultPath);

                using (var archive = new ZipArchive(ms, ZipArchiveMode.Read))
                {
                    archive.ExtractToDirectory(resultPath);
                }

                CreateMappings(resultPath);
                return resultPath;
            }
        }

        private void CreateMappings(string resultPath)
        {
            var mappingData = MappingData.ReadForActivity(EDAResponder.ActivityInstanceID);

            var resultData = new Results();
            var metaDataPath = Path.Combine(resultPath, "metadata.json");
            var metaData = SharedData.Serialization.ReadFromFromFile<MetaData>(metaDataPath);
            var docType = metaData.DocumentType;
            var mappedFields = mappingData.DocumentMappings
                .FirstOrDefault(md => md.DocumentTypeName.Equals(docType, StringComparison.OrdinalIgnoreCase))
                ?.FieldMappings.Where(fm => !string.IsNullOrWhiteSpace(fm.DestinationName))
                .ToList();

            resultData.Name = metaData.DocumentName;
            if (mappedFields != null)
            {
                var fldNames = mappedFields.Select(mf => mf.FieldSource).ToList();
                var fields = metaData.IndexFields.Where(fld => fldNames.Contains(fld.Name));
                foreach (var fld in fields)
                {
                    var mapFld = new Indexfield();
                    mapFld.Name = mappedFields
                        .FirstOrDefault(mf => mf.FieldSource.Equals(fld.Name, StringComparison.OrdinalIgnoreCase))
                        ?.DestinationName;
                    mapFld.Value = fld.Value;
                    resultData.Fields.Add(mapFld);
                }
            }

            SharedData.Serialization.WriteToFileFile(resultData, Path.Combine(resultPath, "Export.json"));
        }

        private async Task PreviewResult(string output)
        {
            var view = new Views.ResultsView
            {
                DataContext = new ResultViewModel(output)
            };

            var res = await DialogHost.Show(view, "RootDialog");
        }


        private bool fetchCommand_canExecute()
        {
            return CanDownload;
        }

        internal async Task ProcessSelected(DtoProcessInfo process)
        {
            await _signalingClient.ListenOnProcessAsync(process);
            ReportNotification($"Listening for all messages for {SharedData.OEM.Process.ToLower()} {process.ProcessID}");
        }

        public async Task LogInChanged(bool isLoggedIn)
        {
            EDAInitializer = null;
            EDAResponder = null;
            if (isLoggedIn)
            {
                try
                {
                    var cfg = SharedData.ClientFactory.FactoryOptions;
                    var settings = new EndpointSettings
                    {
                        ConfigurationService = cfg.ConfigurationServiceEndpoint,
                        ProcessService = cfg.ProcessServiceEndpoint
                    };
                    if (_signalingClient != null) await _signalingClient.DisposeAsync();

                    var factory = new SignalClientFactory(settings);
                    _signalingClient = factory.CreateEventSignalsClient();
                    _signalingClient.RegisterCallback(ReportNotification);

                    await _signalingClient.ConnectAsync();
                    SharedData.MessagingService.ClearMessages();
                    ReportNotification("Established Signal R connection...");
                }
                catch (Exception ex)
                {
                    SharedData.SnackBarMessageQ.Enqueue(
                        $"Failed to initialise the signal R connection: {ex.Message} : {ex.InnerException?.Message}");
                }
            }
            else
            {
                if (_signalingClient != null) await _signalingClient.DisposeAsync();
                Clear();
                ReportNotification("Client disconnected");
            }
        }

        private void ReportNotification(DtoEventDrivenNotification notification)
        {
            SharedData.MessagingService.AddMessage(notification.Message, notification.ID);
        }

        private void ReportNotification(string message)
        {
            SharedData.MessagingService.AddMessage(message);
        }
    }
}
