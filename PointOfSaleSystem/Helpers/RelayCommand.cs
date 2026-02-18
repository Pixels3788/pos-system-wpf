using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;

namespace PointOfSaleSystem.Helpers
{
    internal class RelayCommand<T> : ICommand 
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (parameter is not T t)
                return _canExecute == null;

            return _canExecute == null || _canExecute(t);
        }

        public void Execute(object? parameter)
        {
            if (parameter is T t)
                _execute(t);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

    }

    internal class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }


        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    internal class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _taskFunc;

        private readonly Func<bool> _canExecute;

        public AsyncRelayCommand(Func<Task> task, Func<bool>? canExecute = null)
        {
            _taskFunc = task;
            _canExecute = canExecute;
        }

        public async void Execute(object? parameter)
        {
            try
            {
                await _taskFunc();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occured while executing command {Command}", parameter);
            }
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;


        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    internal class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T, Task> _taskFunc;
        private readonly Func<T, bool>? _canExecute;

        public AsyncRelayCommand(Func<T, Task> taskFunc, Func<T, bool>? canExecute = null)
        {
            _taskFunc = taskFunc;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (parameter is not T t)
                return _canExecute == null;

            return _canExecute == null || _canExecute(t);
        }

        public async void Execute(object? parameter)
        {
            try
            {
                if (parameter is T t)
                    await _taskFunc(t);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while executing command {Command}", parameter);
            }
        }


        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}




