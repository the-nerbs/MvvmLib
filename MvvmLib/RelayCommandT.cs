using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MvvmLib
{
    /// <summary>
    /// An implementation of <see cref="ICommand"/> that relays calls to delegates.
    /// </summary>
    public sealed class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;


        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;


        /// <summary>
        /// Initializes a new instance of <see cref="RelayCommand"/>.
        /// </summary>
        /// <param name="execute">The delegate to execute when the command is invoked.</param>
        public RelayCommand(Action<T> execute)
            : this(execute, null)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="RelayCommand"/>.
        /// </summary>
        /// <param name="execute">The delegate to execute when the command is invoked.</param>
        /// <param name="canExecute">A delegate invoked to check if the command can execute.</param>
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            Contract.RequiresNotNull(execute, nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
        }


        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(T parameter)
        {
            return _canExecute is null
                || _canExecute(parameter);
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute((T)parameter);
        }


        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        public void Execute(T parameter)
        {
            _execute(parameter);
        }

        void ICommand.Execute(object parameter)
        {
            Execute((T)parameter);
        }


        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
