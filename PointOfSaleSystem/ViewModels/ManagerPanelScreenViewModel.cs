using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using PointOfSaleSystem.Helpers;
using PointOfSaleSystem.Models;
using PointOfSaleSystem.Services;
using System.Windows.Input;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using PointOfSaleSystem.Services.Interfaces;
using Serilog;


// View model for the manager panel
namespace PointOfSaleSystem.ViewModels
{
    public class ManagerPanelScreenViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        private readonly IDialogService _dialogService;

        public ICommand NavigateToInventoryEditorCommand { get; }

        public ICommand NavigateBackCommand { get; }

        public ICommand NavigateToMenuEditorCommand { get; }

        public ICommand NavigateToUserEditorCommand { get; }

        public ICommand NavigateToLogsCommand { get; }

        public ManagerPanelScreenViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            NavigateToInventoryEditorCommand = new RelayCommand(NavigateToInventoryEditor);
            NavigateBackCommand = new RelayCommand(NavigateBack);
            NavigateToMenuEditorCommand = new RelayCommand(NavigateToMenuEditor);
            NavigateToUserEditorCommand = new RelayCommand(NavigateToUserEditor);
            NavigateToLogsCommand = new RelayCommand(NavigateToLogs);
        }

        public void NavigateToInventoryEditor()
        {
            try
            {
                _navigationService.Navigate<InventoryEditorScreenViewModel>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred while attempting to navigate to the inventory editor from the manager panel");
                _dialogService.ShowError("Error: An error occurred while trying to open the inventory editor, please try again", "Navigation Error");
            }
        }

        public void NavigateBack()
        {
            try
            {
                _navigationService.Navigate<OrderTakingScreenViewModel>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred while attempting to navigate to the order taking screen from the manager panel");
                _dialogService.ShowError("Error: An error occurred while trying to open the order taking screen, please try again", "Navigation Error");
            }
        }

        public void NavigateToMenuEditor()
        {
            try
            {
                _navigationService.Navigate<EditMenuScreenViewModel>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred while attempting to navigate to the menu editor from the manager panel");
                _dialogService.ShowError("Error: An error occurred while trying to open the menu editor, please try again", "Navigation Error");
            }
        }

        public void NavigateToUserEditor()
        {
            try
            {
                _navigationService.Navigate<UserEditorScreenViewModel>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred while attempting to navigate to the user editor from the manager panel");
                _dialogService.ShowError("Error: An error occurred while trying to open the user editor, please try again", "Navigation Error");
            }
        }

        public void NavigateToLogs()
        {
            try
            {
                _navigationService.Navigate<LogsScreenViewModel>();
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred while attempting to navigate to the logs screen from the manager panel");
                _dialogService.ShowError("Error: An error occurred while trying to open the logs screen, please try again", "Navigation Error");
            }
        }

    }
}
