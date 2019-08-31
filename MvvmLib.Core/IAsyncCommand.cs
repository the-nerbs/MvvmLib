using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmLib
{
    /// <summary>
    /// Defines a command which can be run asynchronously.
    /// </summary>
    public interface IAsyncCommand : ICommand
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns>A task that represents the asynchronous command execution.</returns>
        Task ExecuteAsync(object parameter);
    }
}
