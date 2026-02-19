using System;
using System.Collections.Generic;
using System.Text;
using PointOfSaleSystem.Helpers;
using PointOfSaleSystem.Models;
using PointOfSaleSystem.Services;
using System.Windows.Input;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using PointOfSaleSystem.Services.Interfaces;
using System.Collections.ObjectModel;
using Serilog;


namespace PointOfSaleSystem.ViewModels
{
    public class LogsScreenViewModel : BaseViewModel
    {
        private readonly IActionLogService _actionLogService;

        private readonly INavigationService _navigationService;

        private readonly IDialogService _dialogService;

        private ObservableCollection<ActionLog> _logs;

        public ObservableCollection<ActionLog> Logs
        {
            get => _logs;
            set
            {
                SetProperty(ref _logs, value);
            }
        }

        public ICommand NavigateBackCommand { get; }

        public LogsScreenViewModel(IActionLogService actionLogService, INavigationService navigationService, IDialogService dialogService)
        {
            _actionLogService = actionLogService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _logs = new ObservableCollection<ActionLog>();
            NavigateBackCommand = new RelayCommand(NavigateBack);

            LoadLogs();
        }

        private async void LoadLogs()
        {
            try
            {
                var logs = await _actionLogService.GetActionLogs();

                Logs.Clear();
                foreach (var log in logs)
                {
                    Logs.Add(log);
                }

                Log.Information("Loaded {Count} actions logs into the viewmodel", logs.Count);
            }
            catch (Exception ex) 
            {
                Log.Error(ex, "Error loading action logs in viewmodel");
            }
        }


        public void NavigateBack()
        {
            try
            {
                _navigationService.Navigate<ManagerPanelScreenViewModel>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred while attempting to navigate to the manager panel from the logs screen");
                _dialogService.ShowError("Error: An error occurred while trying to navigate to the manager panel, please try again", "Navigation Error");
            }
        }
    }
}
