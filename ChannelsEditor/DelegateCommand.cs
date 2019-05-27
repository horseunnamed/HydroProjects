using System;
using System.Windows.Input;

namespace ChannelsEditor
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _executeAction;
        private readonly Func<object, bool> _canExecuteFunc;
     
        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _executeAction = execute;
            _canExecuteFunc = canExecute;
        }
     
        public void Execute(object parameter) => _executeAction(parameter);

        public bool CanExecute(object parameter) => _canExecuteFunc == null || _canExecuteFunc(parameter);

        public event EventHandler CanExecuteChanged;
    }
}
