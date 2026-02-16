using PointOfSaleSystem.Services.Interfaces;
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
using System.Collections.ObjectModel;


namespace PointOfSaleSystem.ViewModels
{
    public class InventoryEditorScreenViewModel : BaseViewModel
    {
        private readonly IInventoryMenuCoordinator _menuCoordinator;

        private readonly INavigationService _navigationService;

        private readonly IInventoryService _inventoryService;

        private readonly IActionLogService _actionLogService;

        private ObservableCollection<InventoryItem> _inventoryItems;

        public ObservableCollection<InventoryItem> InventoryItems
        {
            get => _inventoryItems;
            set
            {
                SetProperty(ref _inventoryItems, value);
            }
        }

        private InventoryItem? _selectedInventoryItem;

        public InventoryItem? SelectedInventoryItem
        {
            get => _selectedInventoryItem;
            set
            {
                SetProperty(ref _selectedInventoryItem, value);
            }
        }

        public ICommand NavigateToOrderScreenCommand { get; }

        public ICommand SaveInventoryCommand { get; }

        public InventoryEditorScreenViewModel(IInventoryMenuCoordinator menuCoordinator, INavigationService navigationService, IInventoryService inventoryService, IActionLogService actionLogService)
        {
            _menuCoordinator = menuCoordinator;
            _navigationService = navigationService;
            _inventoryService = inventoryService;
            _actionLogService = actionLogService;
            _inventoryItems = new ObservableCollection<InventoryItem>(_menuCoordinator.ReconstructInventoryItems());
            NavigateToOrderScreenCommand = new RelayCommand(NavigateToOrderScreen);
            SaveInventoryCommand = new RelayCommand(SaveInventory);
            
        }

        public void NavigateToOrderScreen()
        {
            _navigationService.Navigate<ManagerPanelScreenViewModel>();
        }

        public async void SaveInventory()
        {
            if (SelectedInventoryItem != null)
            {
                foreach (var item in InventoryItems)
                {
                    SelectedInventoryItem = item;
                    _inventoryService.ChangeInventoryItemQuantity(SelectedInventoryItem.InventoryItemId, SelectedInventoryItem.QuantityOnHand);
                }
                await _actionLogService.CreateActionLog(_navigationService.CurrentUser, "Modified Inventory", $"{_navigationService.CurrentUser.FirstName + " " + _navigationService.CurrentUser.LastName} modified existing inventory");
            }
        }
    }
}
