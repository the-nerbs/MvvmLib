﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLib
{
    /// <summary>
    /// Provides a data-bindable wrapper for a <see cref="System.Threading.Tasks.Task"/>.
    /// </summary>
    public sealed class TaskExecution : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Gets the task being watched.
        /// </summary>
        public Task Task { get; }

        /// <summary>
        /// Gets a task that completes successfully with the watched task completes.
        /// </summary>
        public Task CompletionTask { get; }

        /// <summary>
        /// Gets the status of the task.
        /// </summary>
        public TaskStatus Status
        {
            get { return Task.Status; }
        }

        /// <summary>
        /// Gets a value indicating if the watched task has completed.
        /// </summary>
        public bool IsCompleted
        {
            get { return Task.IsCompleted; }
        }

        /// <summary>
        /// Gets a value indicating if the task completed successfully.
        /// </summary>
        public bool IsCompletedSuccessfully
        {
            get { return (Task.Status == TaskStatus.RanToCompletion); }
        }

        /// <summary>
        /// Gets a value indicating if the watched task has not yet completed.
        /// </summary>
        public bool IsNotCompleted
        {
            get { return !IsCompleted; }
        }

        /// <summary>
        /// Gets a value indicating if the watched task was canceled.
        /// </summary>
        public bool IsCanceled
        {
            get { return Task.IsCanceled; }
        }

        /// <summary>
        /// Gets a value indicating if the watched task is faulted.
        /// </summary>
        public bool IsFaulted
        {
            get { return Task.IsFaulted; }
        }

        /// <summary>
        /// Gets the exception the watched task faulted with.
        /// </summary>
        public AggregateException Exception
        {
            get { return Task.Exception; }
        }

        /// <summary>
        /// Gets the first inner exception that caused the task to fault.
        /// </summary>
        public Exception InnerException
        {
            get { return Task.Exception?.InnerException; }
        }

        /// <summary>
        /// Gets the collection of exceptions that caused the task to fault.
        /// </summary>
        public IReadOnlyList<Exception> InnerExceptions
        {
            get { return Task.Exception?.InnerExceptions; }
        }


        /// <summary>
        /// Initializes a new instance of <see cref="TaskExecution"/>, wrapping the given task.
        /// </summary>
        /// <param name="task">The task to watch.</param>
        public TaskExecution(Task task)
        {
            Contract.RequiresNotNull(task, nameof(task));

            Task = task;

            if (Task.IsCompleted)
            {
                CompletionTask = Task.CompletedTask;
            }
            else
            {
                CompletionTask = task.ContinueWith(t =>
                {
                    OnTaskStatusChanged();
                });
            }
        }


        private void OnTaskStatusChanged()
        {
            var handler = PropertyChanged;
            if (handler is null)
            {
                return;
            }

            void Raise(string name)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }

            Raise(nameof(Status));
            Raise(nameof(IsCompleted));
            Raise(nameof(IsCompletedSuccessfully));
            Raise(nameof(IsNotCompleted));
            Raise(nameof(IsCanceled));
            Raise(nameof(IsFaulted));
            Raise(nameof(Exception));
            Raise(nameof(InnerException));
            Raise(nameof(InnerExceptions));
        }
    }
}
