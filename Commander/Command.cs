using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Commander
{
    public class Command : ICommand
    {
        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            if (canExecute == null)
                return true;

            return canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
                action(parameter);
        }

        #endregion

        #region Event Handlers

        void notifyObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == propertyName && CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        void notifyObject_PropertyChanged2(object sender, PropertyChangedEventArgs e)
        {
            if (propertyNames == null || CanExecuteChanged == null)
                return;
            if (propertyNames.Contains(e.PropertyName))
                CanExecuteChanged(this, EventArgs.Empty);
        }

        #endregion

        #region Constructor

        public Command()
        {

        }

        /// <summary>
        /// Anlegen des Kommandos
        /// </summary>
        /// <param name="action">Diese Methode wird ausgeführt</param>
        /// <param name="canExecute">Mit Hilfe dieser Methode wird abgefragt, ob das Kommando ausgeführt werden soll, kann weggelassen werden</param>
        /// <param name="notifyObject">Ein Object, dessen Änderung eine Änderung im Command-CanExecute hervorruft</param>
        /// <param name="propertyName">Der Name des Properties, auf das gelauscht wird</param>
        public Command(Action<object> action, Func<object, bool> canExecute = null, INotifyPropertyChanged notifyObject = null, string propertyName = null)
        {
            this.action = action;
            this.canExecute = canExecute;
            this.propertyName = propertyName;
            if (notifyObject != null)
                notifyObject.PropertyChanged += notifyObject_PropertyChanged;
        }

        /// <summary>
        /// Anlegen des Kommandos
        /// </summary>
        /// <param name="action">Diese Methode wird ausgeführt</param>
        /// <param name="canExecute">Mit Hilfe dieser Methode wird abgefragt, ob das Kommando ausgeführt werden soll, kann weggelassen werden</param>
        /// <param name="notifyObject">Ein Object, dessen Änderung eine Änderung im Command-CanExecute hervorruft</param>
        /// <param name="propertyNames">Die Namen der Properties, auf die gelauscht wird</param>
        public Command(Action<object> action, Func<object, bool> canExecute, INotifyPropertyChanged notifyObject, string[] propertyNames)
        {
            this.action = action;
            this.canExecute = canExecute;
            this.propertyNames = propertyNames;
            if (notifyObject != null)
                notifyObject.PropertyChanged += notifyObject_PropertyChanged2;
        }

        #endregion

        #region Methods

        public void InvalidateRequerySuggested()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        #endregion

        #region Fields

        protected Action<object> action;
        protected Func<object, bool> canExecute;
        string propertyName;
        string[] propertyNames;

        #endregion
    }
}
