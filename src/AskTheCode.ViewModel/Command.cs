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

        public Command(Action<object> onExecute, Func<object, bool> onCanExecute = null, string name = null)
        {
            this.onExecute = onExecute;
            this.onCanExecute = onCanExecute;
            this.Name = name;
        }

        public event EventHandler CanExecuteChanged;

        public string Name { get; private set; }

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
    }
}
