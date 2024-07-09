using EDAClient.Common;
using EDAClient.Data;
using EDAClient.Views;
using MaterialDesignThemes.Wpf;
using SampleEventDrivenActivity.Configuration;
using STG.Common.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using STG.Common.DTO.Search;

namespace EDAClient.ViewModels
{
    public class ProcessActionsViewModel : INotifyPropertyChanged
    {
        private readonly Dictionary<int, Description> _descriptionCache = new Dictionary<int, Description>();
        private ICommand _editmappingsCommand;
        private ICommand _editPropertiesCommand;
        private ICommand _selectedProcessChangedCommand;

        public ProcessActionsViewModel()
        {
            EDAInitializer = null;
            EDAResponder = null;
            RuntimeVM = new RuntimeViewModel();
        }

        public string SelectionText { get; } = $"Select a {SharedData.OEM.Process.ToLower()}";

        public string ComboHint { get; } = $"Available {SharedData.OEM.Process.ToLower()}"; 

        public string EditTooltip { get; } = $"Edit {SharedData.OEM.Process} Properties";

        public string MappingsTooltip { get; } = $"Edit {SharedData.OEM.Process} Mappings";

        public Description CurrentDescription { get; set; }

        public RuntimeViewModel RuntimeVM { get; set; }

        public DtoActivityInstanceInfo EDAInitializer { get; set; }

        public DtoActivityInstanceInfo EDAResponder { get; set; }

        public bool FetchingExporter { get; set; }

        public ObservableCollection<DtoProcessInfo> Processes { get; private set; }

        public DtoProcessInfo SelectedProcess { get; set; }

        public bool WarnImporter { get; set; }

        public bool WarnExporter { get; set; }

        public bool IsLoggedIn { get; set; }

        public bool FetchingStarter { get; set; }

        public ICommand SelectedProcessChangedCommand =>
            _selectedProcessChangedCommand ?? (_selectedProcessChangedCommand = new AsyncCommandExecutor((x) => ProcessChanged_execute(), x => true, nameof(SelectedProcessChangedCommand)));

        public ICommand EditPropertiesCommand => _editPropertiesCommand ?? (_editPropertiesCommand =
                                                     new AsyncCommandExecutor(EditPropertiesCommand_execute,
                                                         EditPropertiesCommand_canExecute, nameof(EditPropertiesCommand)));

        public ICommand EditMappingsCommand => _editmappingsCommand ?? (_editmappingsCommand =
                                                   new AsyncCommandExecutor(EditMappingsCommand_execute,
                                                       EditMappingsCommand_canExecute, nameof(EditMappingsCommand)));

        public event PropertyChangedEventHandler PropertyChanged;

        private async Task EditPropertiesCommand_execute()
        {
            MetaDataExtended data;
            try
            {
                FetchingStarter = true;
                data = WarnImporter
                    ? await GetDefaultFromActivity()
                    : ReadSettingsFromFile();
            }
            catch (Exception e)
            {
                SharedData.SnackBarMessageQ.Enqueue(
                    $"Unable to retrieve the EDA initializer definition because: {e.Message} : {e.InnerException?.Message}");
                return;
            }
            finally
            {
                FetchingStarter = false;
            }


            var view = new InputPropertiesView
            {
                DataContext = new InputPropertiesViewModel(data)
            };

            var result = await DialogHost.Show(view, "RootDialog", ImporterDialogClosing);
        }

        private async Task<MetaDataExtended> GetDefaultFromActivity()
        {
            var streamDefinition = await SharedData.EDA.GetActivityDefinitionAsync(EDAInitializer.ActivityInstanceID);

            var ret = new MetaDataExtended();

            var settings = SharedData.Serialization.ReadFromStream<EventDrivenInitializerSettings>(streamDefinition);
            if (settings == null)
                settings = new EventDrivenInitializerSettings();
            ret.DocumentName = settings.RootDocumentName;
            var cv = settings.PrefillCustomValues?.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
            foreach (var name in cv) ret.CustomValues.Add(new CustomValue { Key = name, Value = "<Set data here>" });

            ret.ClassificationURL = settings.ClassificationSettings.ClassificationServiceURL;
            ret.DocClassificationClassCV = settings.ClassificationSettings.DocClassificationProjectCVName;
            ret.DoDocClassification = settings.ClassificationSettings.HaveDocClassifier;
            ret.DoPageClassification = settings.ClassificationSettings.HavePageClassifier;
            ret.PageClassificationClassCV = settings.ClassificationSettings.PageClassificationProjectCVName;

            return ret;
        }

        private MetaDataExtended ReadSettingsFromFile()
        {
            var settingsPath = SharedData.IOService.GetDataFilePath(EDAInitializer.ActivityInstanceID);
            return SharedData.Serialization.ReadFromFromFile<MetaDataExtended>(settingsPath);
        }


        public void ImporterDialogClosing(object sender, DialogClosingEventArgs ea)
        {
            var ipv = ea.Parameter as InputPropertiesViewModel;
            if (ipv != null)
            {
                var validationMsg = ValidateInputData(ipv);
                if (!string.IsNullOrWhiteSpace(validationMsg))
                {
                    SharedData.SnackBarMessageQ.Enqueue(validationMsg);
                    ea.Cancel();
                    return;
                }

                var metaData = new MetaDataExtended();
                metaData.DocumentName = ipv.DocumentName;
                foreach (var cv in ipv.CustomValues) metaData.CustomValues.Add(cv);
                EnrichWithClassificationData(ipv, metaData);
                var path = SharedData.IOService.GetDataFilePath(EDAInitializer.ActivityInstanceID);
                SharedData.Serialization.WriteToFileFile(metaData, path);

                WarnImporter = false;
            }
        }

        private static void EnrichWithClassificationData(InputPropertiesViewModel ipv, MetaDataExtended metaData)
        {
            if (ipv.MustConfigureClassification)
            {
                if (ipv.HasDocumentClassifier)
                {
                    metaData.CustomValues.Add(new CustomValue() { Key = ipv.DocumentClassificationProjectCV, Value = ipv.SelectedDocumentClassificationProject });
                }
                if (ipv.HasPageClassifier)
                {
                    metaData.CustomValues.Add(new CustomValue() { Key = ipv.PageClassificationProjectCV, Value = ipv.SelectedPageClassificationProject });
                }
                metaData.ClassificationURL = ipv.ClassificationServiceURL;
                metaData.VCPURL = ipv.VCPCURL;
                metaData.DocClassificationClassCV = ipv.DocumentClassificationProjectCV;
                metaData.PageClassificationClassCV = ipv.PageClassificationProjectCV;
                metaData.DoDocClassification = ipv.HasDocumentClassifier;
                metaData.DoPageClassification = ipv.HasPageClassifier;
            }
        }

        private string ValidateInputData(InputPropertiesViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel.DocumentName))
            {
                return "The document name is required";
            }

            if (viewModel.MustConfigureClassification)
            {
                if (viewModel.HasDocumentClassifier)
                {
                    if (string.IsNullOrWhiteSpace(viewModel.SelectedDocumentClassificationProject))
                    {
                        return "The document classification project must be selected";
                    }
                }
                if (viewModel.HasPageClassifier)
                {
                    if (string.IsNullOrWhiteSpace(viewModel.SelectedPageClassificationProject))
                    {
                        return "The page classification project must be selected";
                    }
                }

            }

            return string.Empty;
        }

        private bool EditPropertiesCommand_canExecute()
        {
            return EDAInitializer != null;
        }

        private async Task EditMappingsCommand_execute()
        {
            MappingData data;
            try
            {
                FetchingExporter = true;
                data = WarnExporter
                    ? await GetMappingsFromActivity()
                    : await Task.Run(() => { return MappingData.ReadForActivity(EDAResponder.ActivityInstanceID); });
            }
            catch (Exception e)
            {
                SharedData.SnackBarMessageQ.Enqueue(
                    $"Unable to retrieve the EDA responder mappings definition because: {e.Message} : {e.InnerException?.Message}");
                return;
            }
            finally
            {
                FetchingExporter = false;
            }


            var view = new OutputMappingsView
            {
                DataContext = new OutputMappingsViewModel(data)
            };

            var result = await DialogHost.Show(view, "RootDialog", ExporterDialogClosing);
        }

        private async Task<MappingData> GetMappingsFromActivity()
        {
            var docTypes = await SharedData.EDA.GetDocumentTypesForProcessAsync(EDAInitializer.Process.ProcessID);
            var ret = new MappingData();

            foreach (var docType in docTypes)
            {
                var map = new DocumentMapping
                {
                    DocumentTypeName = docType.Name
                };
                foreach (var fld in docType.FieldDefinitions)
                    map.FieldMappings.Add(new FieldMapping { FieldSource = fld.Name, DestinationName = fld.Name });
                ret.DocumentMappings.Add(map);
            }

            return ret;
        }


        public void ExporterDialogClosing(object sender, DialogClosingEventArgs ea)
        {
            var omvm = ea.Parameter as OutputMappingsViewModel;
            if (omvm != null)
            {
                var md = new MappingData();
                foreach (var dm in omvm.DocumentTypes) md.DocumentMappings.Add(dm);

                var path = SharedData.IOService.GetDataFilePath(EDAResponder.ActivityInstanceID);
                SharedData.Serialization.WriteToFileFile(md, path);

                WarnExporter = false;
            }
        }

        private bool EditMappingsCommand_canExecute()
        {
            return EDAResponder != null;
        }

        public async Task LogInChanged(bool isLoggedIn)
        {
            await RuntimeVM.LogInChanged(isLoggedIn);
            EDAInitializer = null;
            EDAResponder = null;
            CurrentDescription = null;
            if (isLoggedIn)
            {
                try
                {
                    PropertyChanged += ProcessActionsViewModel_PropertyChanged;
                    var processes = await SharedData.EDA.GetEventDrivenProcessesAsync(Guid.Empty);
                    Processes = new ObservableCollection<DtoProcessInfo>(processes);
                    IsLoggedIn = true;
                }
                catch (Exception ex)
                {
                    SharedData.SnackBarMessageQ.Enqueue(
                        $"Unable to retrieve the EDA {SharedData.OEM.Process} because: {ex.Message} : {ex.InnerException?.Message}");
                }
            }
            else
            {
                PropertyChanged -= ProcessActionsViewModel_PropertyChanged;
                if (Processes != null) Processes.Clear();
                IsLoggedIn = false;
            }
        }


        private void ProcessActionsViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WarnImporter))
            {
                if (EDAInitializer != null)
                    RuntimeVM.CanUpload = !WarnImporter;
                else
                    RuntimeVM.CanUpload = false;
            }
            else if (e.PropertyName == nameof(WarnExporter))
            {
                if (EDAResponder != null)
                    RuntimeVM.CanDownload = !WarnExporter;
                else
                    RuntimeVM.CanDownload = false;
            }
        }

        private async Task ProcessChanged_execute()
        {
            Clear();
            await InitEda();
        }

        private void Clear()
        {
            EDAInitializer = null;
            EDAResponder = null;
            WarnImporter = false;
            WarnExporter = false;
            RuntimeVM.EDAInitializer = null;
            RuntimeVM.EDAResponder = null;
            CurrentDescription = null;
        }

        private async Task InitEda()
        {
            if (SelectedProcess == null)
                return;

            var allEDA = await SharedData.EDA.FindEventDrivenActivities(SelectedProcess.ProcessID);
            if (allEDA == null)
            {
                SharedData.SnackBarMessageQ.Enqueue($"The selected {SharedData.OEM.Process.ToLower()} does not contain any event driven activity");
            }
            else
            {
                var starters = allEDA.Where(ai => ai.ExecutionType == DtoActivityExecutionType.EventDrivenInitializer).ToList();
                var reporters = allEDA.Where(ai => ai.ExecutionType == DtoActivityExecutionType.EventDrivenResponder).ToList();

                var validationMsg = ValidateProcess(starters, reporters);
                if (!string.IsNullOrWhiteSpace(validationMsg))
                {
                    SharedData.SnackBarMessageQ.Enqueue(validationMsg);
                    return;
                }

                EDAInitializer = starters.FirstOrDefault();
                EDAResponder = reporters.FirstOrDefault();
                RuntimeVM.EDAResponder = EDAResponder;
                RuntimeVM.EDAInitializer = EDAInitializer;

                await ProvideDescription();

                WarnImporter = !File.Exists(SharedData.IOService.GetDataFilePath(EDAInitializer.ActivityInstanceID));
                RuntimeVM.CanUpload = !WarnImporter;

                if (EDAResponder != null)
                {
                    WarnExporter = !File.Exists(SharedData.IOService.GetDataFilePath(EDAResponder.ActivityInstanceID));
                    RuntimeVM.CanDownload = !WarnExporter;
                }

                await RuntimeVM.ProcessSelected(SelectedProcess);
            }
        }

        private string ValidateProcess(IList<DtoActivityInstanceInfo> starters, IList<DtoActivityInstanceInfo> reporters)
        {
            if (starters.Count == 0)
                return
                    $"The selected {SharedData.OEM.Process.ToLower()} does not contain any event driven initializer activity. This application does not support such scenario";
            if (starters.Count > 1)
                return
                    $"The selected {SharedData.OEM.Process.ToLower()} contains {starters.Count} event driven initializer activities. This application does not support such scenario";

            if (reporters.Count > 1)
                return
                    $"The selected {SharedData.OEM.Process.ToLower()} contains {reporters.Count} event driven responder activities. This application does not support such scenario";

            return string.Empty;
        }


        private async Task ProvideDescription()
        {
            if (_descriptionCache.ContainsKey(EDAInitializer.ActivityInstanceID))
            {
                CurrentDescription = _descriptionCache[EDAInitializer.ActivityInstanceID];
            }
            else
            {
                var streamDefinition = await SharedData.EDA.GetActivityDefinitionAsync(EDAInitializer.ActivityInstanceID);
                var desc = new Description();
                var settings =
                    SharedData.Serialization.ReadFromStream<EventDrivenInitializerSettings>(streamDefinition);
                if (settings == null)
                    settings = new EventDrivenInitializerSettings();
                desc.Short = settings.ShortDescription;
                desc.Long = settings.LongDescription;

                _descriptionCache.Add(EDAInitializer.ActivityInstanceID, desc);
                CurrentDescription = desc;
            }
        }
    }
}