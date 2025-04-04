using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using EDAClient.Data;
using STG.Common.DTO.EventDriven;

namespace EDAClient.ViewModels
{
    public class SelectNotificationViewModel : INotifyPropertyChanged
    {
        public SelectNotificationViewModel(IList<ExtendedNotification> notifications)
        {
            Notifications = new ObservableCollection<ExtendedNotification>(notifications);
        }

        public ObservableCollection<ExtendedNotification> Notifications { get; }

        public DtoEventDrivenNotification SelectedNotification { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}