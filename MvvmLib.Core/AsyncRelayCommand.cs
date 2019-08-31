using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmLib
{
    /// <summary>
    /// Provides an implementation of <see cref="IAsyncCommand"/> which invokes delegates it is
    /// constructed with.
    /// </summary>
    public sealed class AsyncRelayCommand : IAsyncCommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;


        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;


        /// <summary>
        /// Initializes a new instance of <see cref="AsyncRelayCommand"/>.
        /// </summary>
        /// <param name="execute">The delegate to invoke when this command is executed.</param>
        public AsyncRelayCommand(Func<Task> execute)
            : this(execute, null)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="AsyncRelayCommand"/>.
        /// </summary>
        /// <param name="execute">The delegate to invoke when this command is executed.</param>
        /// <param name="canExecute">
        /// The delegate to invoke to determine if this command can be executed. If null, the 
        /// command can always be executed.
        /// </param>
        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute)
        {
            Contract.RequiresNotNull(execute, nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
        }


        /// <summary>
        /// Determines if this command can be executed.
        /// </summary>
        /// <returns>True if it is valid to execute this command, or false if not.</returns>
        public bool CanExecute()
        {
            return _canExecute is null
                || _canExecute();
        }

        /// <summary>
        /// Executes this command synchronously.
        /// </summary>
        public void Execute()
        {
            ExecuteAsync().Wait();
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>A task that represents the asynchronous command execution.</returns>
        public Task ExecuteAsync()
        {
            return _execute();
        }


        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }


        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute();
        }

        void ICommand.Execute(object parameter)
        {
            Execute();
        }

        Task IAsyncCommand.ExecuteAsync(object parameter)
        {
            return ExecuteAsync();
        }
    }
}
