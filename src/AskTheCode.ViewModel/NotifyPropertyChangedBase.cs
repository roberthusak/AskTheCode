using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.ViewModel
{
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged<T>(string propertyName, T previousValue)
        {
        }

        /// <summary>
        /// Checks if a property already matches a desired value.  Sets the property and notifies listeners only when
        /// necessary.
        /// </summary>
        /// <seealso href="https://blogs.msdn.microsoft.com/msgulfcommunity/2013/03/13/understanding-the-basics-of-mvvm-design-pattern/">
        /// Original code on MSDN blog</seealso>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners.  This value is optional and can
        /// be provided automatically when invoked from compilers that support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the desired value.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            Contract.Requires<ArgumentNullException>(propertyName != null, nameof(propertyName));

            if (object.Equals(storage, value))
            {
                return false;
            }

            T previousValue = storage;
            storage = value;
            this.OnPropertyChanged<T>(propertyName, previousValue);
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
