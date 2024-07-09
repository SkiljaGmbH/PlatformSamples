using System.Windows;
using EDAClient.Data;
using EDAClient.ViewModels;

namespace EDAClient.Common
{
    internal class MessagingService
    {
        private readonly MessagesViewModel _messagesViewModel;

        public MessagingService(MessagesViewModel messagesViewModel)
        {
            _messagesViewModel = messagesViewModel;
        }

        public void ClearMessages()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _messagesViewModel.Messages.Clear();
            });
        }

        public void AddMessage(string msg, int id = -1)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var md = new MessageData() {
                    IsLink = msg.Trim().StartsWith("http"),
                    Message = msg ,
                    NotificationID = id
                };
                _messagesViewModel.Messages.Insert(0, md);
            });
        }
    }
}