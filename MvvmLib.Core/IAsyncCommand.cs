using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmLib
{
    /// <summary>
    /// Defines a command which can be run asynchronously.
    /// </summary>
    public interface IAsyncCommand : ICommand, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the current or most recent execution of the task.
        /// </summary>
        /// <remarks>
        /// If the command has not yet been executed, this property will return null. Once the
        /// command has been executed, this will not be null.
        /// </remarks>
        TaskExecution Execution { get; }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns>A task that represents the asynchronous command execution.</returns>
        Task ExecuteAsync(object parameter);
    }
}
