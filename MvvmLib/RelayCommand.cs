using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MvvmLib
{
    /// <summary>
    /// An implementation of <see cref="ICommand"/> that relays calls to delegates.
    /// </summary>
    public sealed class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;


        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;


        /// <summary>
        /// Initializes a new instance of <see cref="RelayCommand"/>.
        /// </summary>
        /// <param name="execute">The delegate to execute when the command is invoked.</param>
        public RelayCommand(Action execute)
            : this(execute, null)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="RelayCommand"/>.
        /// </summary>
        /// <param name="execute">The delegate to execute when the command is invoked.</param>
        /// <param name="canExecute">A delegate invoked to check if the command can execute.</param>
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            Contract.RequiresNotNull(execute, nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
        }


        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute()
        {
            return _canExecute is null
                || _canExecute();
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute();
        }


        /// <summary>
        /// Executes the command.
        /// </summary>
        public void Execute()
        {
            _execute();
        }

        void ICommand.Execute(object parameter)
        {
            Execute();
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
