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

namespace PointOfSaleSystem.ViewModels
{
    public class LogsScreenViewModel : BaseViewModel
    {
        private readonly IActionLogService _actionLogService;

        private readonly INavigationService _navigationService;

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

        public LogsScreenViewModel(IActionLogService actionLogService, INavigationService navigationService)
        {
            _actionLogService = actionLogService;
            _navigationService = navigationService;
            _logs = new ObservableCollection<ActionLog>(_actionLogService.GetActionLogs());
            NavigateBackCommand = new RelayCommand(NavigateBack);
        }


        public void NavigateBack()
        {
            _navigationService.Navigate<ManagerPanelScreenViewModel>();
        }
    }
}
