using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AskTheCode.ViewModel
{
    public class Command : ICommand
    {
        private readonly Action<object> onExecute;
        private readonly Func<object, bool> onCanExecute;

        public Command(Action onExecute, Func<bool> onCanExecute = null)
            : this(
                  (parameter) => onExecute(),
                  (onCanExecute != null) ? new Func<object, bool>((parameter) => onCanExecute()) : null)
        {
        }

        public Command(Action<object> onExecute, Func<object, bool> onCanExecute = null)
        {
            this.onExecute = onExecute;
            this.onCanExecute = onCanExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (this.onCanExecute == null)
            {
                return true;
            }
            else
            {
                return this.onCanExecute(parameter);
            }
        }

        public void Execute(object parameter)
        {
            this.onExecute(parameter);
        }

        public void NotifyCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
}
