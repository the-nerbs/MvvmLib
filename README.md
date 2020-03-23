MvvmLib
-------------------------------------------------------------------------------

MvvmLib is a lightweight .NET library to help reduce some of the boilerplate of MVVM applications.

Some of the features of this library are:
- View model base classes with validation
- Asynchronous commands with observable results
- MvvmLight compatibility

### View model base classes

This library exposes two base classes for view model types, `ViewModel` and `ValidatingViewModel`.
`ViewModel` provides a base class that provides an implementation of `INotifyPropertyChanged`
as well as tracking a service container for test isolation.  `ValidatingViewModel` extends the
base `ViewModel` by providing an implementation of `INotifyDataErrorInfo` for WPF validation support.


### Asynchronous commands

The `AsyncRelayCommand` and `AsyncRelayCommand<T>` types provide implementations of the 
`ICommand` interface that defers to delegates returning `Task`s. These commands also track their
most recent execution in a `TaskExecution` or `TaskExecution<T>`, allowing you to use WPF's
ordinary data binding mechanisms to signal the UI with the command's results, or any error that
occurred.

These command types also allow you to write commands following the normal async-await patterns,
which allows you to write tests against your view model's command without needing to worry about
synchronizing or waiting for async-void command implementations to complete.


### MvvmLight compatibility

All of the features of this library are available in a standalone package that has no dependencies
called `MvvmLib`.  Along with this is the package `MvvmLib.MvvmLight` which provides
nearly-identical types that instead use the `MvvmLight` types.

### IOC Container

An IOC container is provided as an optional component through the `MvvmLib.Ioc` package. The 
`IocContainer` class implements the `IServiceLocator` interface from `CommonServiceLocator`.
