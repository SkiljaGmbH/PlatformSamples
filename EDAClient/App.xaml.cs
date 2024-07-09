using System;
using System.Windows;
using System.Windows.Threading;
using EDAClient.ViewModels;

namespace EDAClient
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ApplicationViewModel _mainVM;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // create the main window and assign the main view model as data context
            var main = new MainWindow();

            // run the application
            _mainVM = new ApplicationViewModel(main.MainSnackbar.MessageQueue);

            //Handle the closure of the app.
            EventHandler handler = null;
            handler = delegate
            {
                _mainVM.RequestClose -= handler;
                main.Close();
            };
            _mainVM.RequestClose += handler;

            main.DataContext = _mainVM;
            main.Show();
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;
            e.Handled = true;

            var msg = ex.Message;
            var stackTrace = ex.StackTrace;

            MessageBox.Show(string.Format("Message: {0} " + Environment.NewLine + "StackTrace: {1}", msg, stackTrace),
                "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }


        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (_mainVM != null) _mainVM = null;
        }
    }
}
