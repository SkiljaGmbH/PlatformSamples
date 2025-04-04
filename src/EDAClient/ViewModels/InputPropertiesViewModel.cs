using EDAClient.Common;
using EDAClient.Data;
using MaterialDesignThemes.Wpf;
using SampleEventDrivenActivity.Configuration;
using Skilja.Stelvio.Classification.ProjectService.Client;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace EDAClient.ViewModels
{
    public class InputPropertiesViewModel : INotifyPropertyChanged
    {
        private ICommand _addCVCommand;
        private ICommand _deleteCVCommand;
        private ICommand _editCVCommand;
        private ICommand _connectClsSvcCommand;
        private ICommand _editClassificationCommand;
        private bool _isEdit;

        public InputPropertiesViewModel(MetaDataExtended loadedData)
        {
            if (loadedData == null)
                loadedData = new MetaDataExtended();

            ClassificationServiceURL = loadedData.ClassificationURL;
            VCPCURL = loadedData.VCPURL;
            HasDocumentClassifier = loadedData.DoDocClassification;
            HasPageClassifier = loadedData.DoPageClassification;
            DocumentClassificationProjectCV = loadedData.DocClassificationClassCV;
            PageClassificationProjectCV = loadedData.PageClassificationClassCV;


            MustConfigureClassification = loadedData.DoDocClassification || loadedData.DoPageClassification;
            DocumentName = loadedData.DocumentName;
            CustomValues = new ObservableCollection<CustomValue>(loadedData.CustomValues);
            DialogCV = new CustomValue();
            ClearClassificationData();
        }

        public string ClassificationServiceURL { get; set; }
        public string VCPCURL { get; set; }
        public bool HasDocumentClassifier { get; }
        public bool HasPageClassifier { get; }
        internal string DocumentClassificationProjectCV { get; }
        internal string PageClassificationProjectCV { get; }

        public ObservableCollection<string> ClassificationProjects { get; } = new ObservableCollection<string>();
        public string SelectedDocumentClassificationProject { get; set; }
        public string SelectedPageClassificationProject { get; set; }

        public bool IsClsConnected { get; set; }

        public bool IsClsConnecting { get; set; }

        public bool MustConfigureClassification { get; }
        public ObservableCollection<CustomValue> CustomValues { get; }

        public string DocumentName { get; set; }

        public CustomValue DialogCV { get; set; }

        public ICommand DeleteCVCommand =>
            _deleteCVCommand ?? (_deleteCVCommand = new CommandExecutor(deleteCVCommand_execute));

        public ICommand EditCVCommand =>
            _editCVCommand ?? (_editCVCommand = new CommandExecutor(editCVCommand_execute));

        public ICommand AddCVCommand => _addCVCommand ?? (_addCVCommand = new CommandExecutor(addCVCommand_execute));

        public ICommand ConnectClsSvcCommand => _connectClsSvcCommand ?? (_connectClsSvcCommand = new CommandExecutor(connectClsSvcCommand_execute, connectClsSvcCommand_canExecute));


        public ICommand EditClassificationCommand => _editClassificationCommand ?? (_editClassificationCommand = new CommandExecutor(editClassificationCommand_execute, editClassificationCommand_canExecute));



        public event PropertyChangedEventHandler PropertyChanged;


        private void editClassificationCommand_execute(object o)
        {
            var name = o as string;
            if (!string.IsNullOrWhiteSpace(name))
            {
                var c = GetClient();
                if (c != null)
                {
                    var selected = c.GetProjects().FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (selected != null)
                    {
                        var url = $"{ VCPCURL}?projectId={selected.ID}";
                        url = url.Trim();
                        if (!(url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                        {
                            url = $"http://{url}";
                        }
                        ProcessStartInfo sInfo = new ProcessStartInfo(url);
                        Process.Start(sInfo);
                    }
                }
            }

        }

        private bool editClassificationCommand_canExecute(object o)
        {
            if (string.IsNullOrWhiteSpace(VCPCURL) || o == null)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        private void deleteCVCommand_execute(object o)
        {
            var cv = o as CustomValue;

            if (cv != null) CustomValues.Remove(cv);
        }

        private void editCVCommand_execute(object o)
        {
            openCVDialog(true, o as CustomValue);
        }

        private void addCVCommand_execute(object o)
        {
            DialogCV = new CustomValue();

            openCVDialog(false, DialogCV);
        }

        private void connectClsSvcCommand_execute(object o)
        {
            IsClsConnecting = true;
            try
            {
                LoadClassificationProjects();
            }
            finally
            {
                IsClsConnecting = false;
            }
        }
        private bool connectClsSvcCommand_canExecute(object o)
        {
            return !string.IsNullOrWhiteSpace(ClassificationServiceURL);
        }

        private void openCVDialog(bool isEdit, CustomValue cv)
        {
            if (cv != null)
            {
                DialogCV = cv;
                _isEdit = isEdit;
                DialogHost.Show(DialogCV, "CVDialog");
            }
        }

        public void DialogClosing(object sender, DialogClosingEventArgs ea)
        {
            if ((bool)ea.Parameter == false)
            {
                DialogCV = new CustomValue();
                _isEdit = false;
                return;
            }

            var validation = validateCV();
            if (!string.IsNullOrWhiteSpace(validation))
            {
                SharedData.SnackBarMessageQ.Enqueue(validation);
                ea.Cancel();
                return;
            }

            if (!_isEdit) CustomValues.Add(new CustomValue { Key = DialogCV.Key, Value = DialogCV.Value });
            DialogCV = new CustomValue();
            _isEdit = false;
        }

        private string validateCV()
        {
            if (string.IsNullOrWhiteSpace(DialogCV.Key) || string.IsNullOrWhiteSpace(DialogCV.Value))
                return "Both Name and value are required for the custom value definition";

            if (!_isEdit && CustomValues.Count(cv => cv.Key.Equals(DialogCV.Key, StringComparison.OrdinalIgnoreCase)) >
                0) return $"A custom value with the Name '{DialogCV.Key}' is allready present";

            return string.Empty;
        }

        private void ClearClassificationData()
        {
            IsClsConnected = false;
            IsClsConnecting = false;
            ClassificationProjects.Clear();
            SelectedDocumentClassificationProject = null;
            SelectedPageClassificationProject = null;
            if (MustConfigureClassification)
            {
                if (HasPageClassifier)
                {
                    var cv = CustomValues.FirstOrDefault(v => v.Key.Equals(PageClassificationProjectCV, StringComparison.OrdinalIgnoreCase));
                    if (cv != null)
                    {
                        ClassificationProjects.Add(cv.Value);
                        SelectedPageClassificationProject = cv.Value;
                        CustomValues.Remove(cv);
                    }
                }
                if (HasDocumentClassifier)
                {
                    var cv = CustomValues.FirstOrDefault(v => v.Key.Equals(DocumentClassificationProjectCV, StringComparison.OrdinalIgnoreCase));
                    if (cv != null)
                    {
                        ClassificationProjects.Add(cv.Value);
                        SelectedDocumentClassificationProject = cv.Value;
                        CustomValues.Remove(cv);
                    }
                }
            }
        }
        private void LoadClassificationProjects()
        {
            ClassificationProjects.Clear();
            try
            {
                if (IsClsConnected)
                {
                    ClearClassificationData();
                }
                else
                {
                    var client = GetClient();
                    if (client != null)
                    {
                        client.GetProjects().ForEach(itm => ClassificationProjects.Add(itm.Name));
                        IsClsConnected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                SharedData.SnackBarMessageQ.Enqueue($"Unable to load classification projects. The reason is: {ex.Message}");
            }
        }

        private ProjectClient GetClient()
        {
            try
            {
                var uri = new Uri(AppendTrailingSlash(ClassificationServiceURL));
                return new ProjectClient(uri);
            }
            catch (Exception ex)
            {
                SharedData.SnackBarMessageQ.Enqueue($"The provided service address is not in the correct format - {ex.Message}");
            }

            return null;
        }

        private string AppendTrailingSlash(string input)
        {
            var k = input.Trim();
            if (k.EndsWith("/"))
                return k;
            return k + "/";
        }
    }
}