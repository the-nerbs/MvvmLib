using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmLib
{
    /// <summary>
    /// Provides an implementation of <see cref="IAsyncCommand"/> which invokes delegates it is
    /// constructed with.
    /// </summary>
    /// <typeparam name="TParameter">The command's parameter type.</typeparam>
    public sealed class AsyncRelayCommand<TParameter> : IAsyncCommand
    {
        private readonly Func<TParameter, Task> _execute;
        private readonly Func<TParameter, bool> _canExecute;
        private TaskExecution _execution;


        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Gets the current or most recent execution of the task.
        /// </summary>
        /// <remarks>
        /// If the command has not yet been executed, this property will return null. Once the
        /// command has been executed, this will not be null.
        /// </remarks>
        public TaskExecution Execution
        {
            get { return _execution; }
            private set
            {
                _execution = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Execution)));
            }
        }


        /// <summary>
        /// Initializes a new instance of <see cref="AsyncRelayCommand"/>.
        /// </summary>
        /// <param name="execute">The delegate to invoke when this command is executed.</param>
        public AsyncRelayCommand(Func<TParameter, Task> execute)
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
        public AsyncRelayCommand(Func<TParameter, Task> execute, Func<TParameter, bool> canExecute)
        {
            Contract.RequiresNotNull(execute, nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
        }


        /// <summary>
        /// Determines if this command can be executed.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <returns>True if it is valid to execute this command, or false if not.</returns>
        public bool CanExecute(TParameter parameter)
        {
            return _canExecute is null
                || _canExecute(parameter);
        }

        /// <summary>
        /// Executes this command synchronously.
        /// </summary>
        public async void Execute(TParameter parameter)
        {
            await ExecuteAsync(parameter);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <returns>A task that represents the asynchronous command execution.</returns>
        public Task ExecuteAsync(TParameter parameter)
        {
            Task t = _execute(parameter);
            Execution = new TaskExecution(t);
            return t;
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
            return CanExecute((TParameter)parameter);
        }

        void ICommand.Execute(object parameter)
        {
            Execute((TParameter)parameter);
        }

        Task IAsyncCommand.ExecuteAsync(object parameter)
        {
            return ExecuteAsync((TParameter)parameter);
        }
    }
}
