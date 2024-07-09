using EDAClient.Common;
using EDAClient.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace EDAClient.ViewModels
{
    public class MessagesViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        

        public MessagesViewModel()
        {
            Messages = new ObservableCollection<MessageData>();
        }

        public ObservableCollection<MessageData> Messages { get; set; }

       
    }
}