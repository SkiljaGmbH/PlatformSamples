using System.Collections.ObjectModel;
using System.ComponentModel;
using EDAClient.Data;

namespace EDAClient.ViewModels
{
    internal class OutputMappingsViewModel : INotifyPropertyChanged
    {
        public OutputMappingsViewModel(MappingData loadedData)
        {
            DocumentTypes = new ObservableCollection<DocumentMapping>(loadedData.DocumentMappings);
        }

        public ObservableCollection<DocumentMapping> DocumentTypes { get; }

        public DocumentMapping SelectedDocumentType { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}