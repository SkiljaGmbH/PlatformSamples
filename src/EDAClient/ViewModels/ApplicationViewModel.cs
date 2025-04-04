using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using EDAClient.Common;
using MaterialDesignThemes.Wpf;

namespace EDAClient.ViewModels
{
    internal class ApplicationViewModel : INotifyPropertyChanged
    {
        private ICommand _exitCommand;

        public ApplicationViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            SharedData.SnackBarMessageQ =
                snackbarMessageQueue ?? throw new ArgumentNullException(nameof(snackbarMessageQueue));

            LoginVM = new LoginViewModel(NotifyLoggedIn);
            ProcessActionsVM = new ProcessActionsViewModel();
            MessagesVM = new MessagesViewModel();
            SharedData.MessagingService = new MessagingService(MessagesVM);
            Title = $"{SharedData.OEM.Brand } EDA";
            Description = $"{SharedData.OEM.Brand } EDA Consumer Application Demo";
            var ph = new PaletteHelper();
            var theme = new Theme();
            theme.SetBaseTheme(BaseTheme.Light);
            ph.SetTheme(Theme.Create(theme.GetBaseTheme(), SharedData.OEM.PrimaryColor, SharedData.OEM.AccentColor));
        }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public MessagesViewModel MessagesVM { get; set; }

        public ICommand ExitCommand => _exitCommand ?? (_exitCommand = new CommandExecutor(exitCommand_execute));


        public LoginViewModel LoginVM { get; set; }

        public ProcessActionsViewModel ProcessActionsVM { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler RequestClose;

        internal async Task NotifyLoggedIn(bool isLoggedIn)
        {
            await ProcessActionsVM.LogInChanged(isLoggedIn);
        }

        private void exitCommand_execute(object e)
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
