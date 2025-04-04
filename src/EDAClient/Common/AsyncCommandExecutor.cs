using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EDAClient.Common
{
    public class AsyncCommandExecutor : IAsyncCommand
    {
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        private bool _isExecuting;
        private readonly Func<object, Task> _execute;
        private readonly Func<object, bool> _canExecute;
        private readonly IErrorHandler _errorHandler;

        public AsyncCommandExecutor(Func<object, Task> execute,
            Func<object, bool> canExecute, string commandName)
        {
            _execute = execute;
            _canExecute = canExecute;
            _errorHandler = new ErrorHandler(commandName);
        }

        public AsyncCommandExecutor(
            Func<Task> execute,
            Func<bool> canExecute, string commandName)
        {
            _execute = _ => execute();
            if (canExecute != null)
                _canExecute = _ => canExecute();
            _errorHandler = new ErrorHandler(commandName);
        }

        public bool CanExecute(object parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);
        }

        public async Task ExecuteAsync(object parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    _isExecuting = true;
                    await _execute(parameter);
                }
                finally
                {
                    _isExecuting = false;
                }
            }

            RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute(parameter);
        }

        void ICommand.Execute(object parameter)
        {
            ExecuteAsync(parameter).FireAndForgetSafeAsync(_errorHandler);
        }
    }

    public interface IErrorHandler
    {
        void HandleError(Exception ex);
    }

    public class ErrorHandler : IErrorHandler
    {
        private readonly string _name;

        public ErrorHandler(string name)
        {
            _name = name;
        }
        public void HandleError(Exception ex)
        {
            var e = ex;
            if (e is AggregateException ag && ag.InnerException != null)
            {
                e = ag.InnerException;
            }
            SharedData.SnackBarMessageQ.Enqueue($"Error caught in command {_name}: {e.Message}");
        }
    }

    public static class TaskUtilities
    {
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public static async void FireAndForgetSafeAsync(this Task task, IErrorHandler handler = null)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                handler?.HandleError(ex);
            }
        }
    }
}