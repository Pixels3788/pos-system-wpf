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

namespace PointOfSaleSystem.ViewModels
{
    public class ManagerPanelScreenViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        public ICommand NavigateToInventoryEditorCommand { get; }

        public ICommand NavigateBackCommand { get; }

        public ICommand NavigateToMenuEditorCommand { get; }

        public ICommand NavigateToUserEditorCommand { get; }

        public ManagerPanelScreenViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            NavigateToInventoryEditorCommand = new RelayCommand(NavigateToInventoryEditor);
            NavigateBackCommand = new RelayCommand(NavigateBack);
            NavigateToMenuEditorCommand = new RelayCommand(NavigateToMenuEditor);
            NavigateToUserEditorCommand = new RelayCommand(NavigateToUserEditor);
        }

        public void NavigateToInventoryEditor()
        {
            _navigationService.Navigate<InventoryEditorScreenViewModel>();
        }

        public void NavigateBack()
        {
            _navigationService.Navigate<OrderTakingScreenViewModel>();
        }

        public void NavigateToMenuEditor()
        {
            _navigationService.Navigate<EditMenuScreenViewModel>();
        }

        public void NavigateToUserEditor()
        {
            _navigationService.Navigate<UserEditorScreenViewModel>();
        }

    }
}
