using System;
using System.Windows.Input;

namespace CommonTools.OpeningClient.Support
{
    public class RelayCommand<T> : ICommand
    {
        private Action<T> _targetExecuteMethod;
        private Func<T, bool> _targetCanExecuteMethod;
        private ICommand cancelCommand;

        public RelayCommand(Action<T> executeMethod)
        {
            _targetExecuteMethod = executeMethod;
        }

        public RelayCommand(ICommand cancelCommand)
        {
            this.cancelCommand = cancelCommand;
        }

        public RelayCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
        {
            _targetExecuteMethod = executeMethod;
            _targetCanExecuteMethod = canExecuteMethod;
        }

        #region implement interface

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (_targetCanExecuteMethod != null) {
                T param = (T)parameter;
                return _targetCanExecuteMethod(param);
            }
            if (_targetExecuteMethod != null) {
                return true;
            }
            return false;
        }

        public void Execute(object parameter)
        {
            if (_targetExecuteMethod != null) {
                T param = (T)parameter;
                //if (param != null)
                _targetExecuteMethod(param);
            }
        }

        #endregion implement interface
    }
}