using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EDAClient.Common
{
    internal class CommandExecutor : ICommand
    {
        private readonly Func<object, bool> _canExecute;
        private readonly Action<object> _execute;

        public CommandExecutor(Action<object> execute) : this(execute, null)
        {
        }

        public CommandExecutor(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? (x => true);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            try
            {
                _execute(parameter);
            }
            catch (Exception ex)
            {
                var e = ex;
                if (e is AggregateException ag && ag.InnerException != null)
                {
                    e = ag.InnerException;
                }
                SharedData.SnackBarMessageQ.Enqueue("Error caught: " + e.Message);
            }
        }
    }
}