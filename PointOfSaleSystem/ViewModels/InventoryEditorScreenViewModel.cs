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
using Serilog;

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
            _inventoryItems = new ObservableCollection<InventoryItem>();
            NavigateToOrderScreenCommand = new RelayCommand(NavigateToOrderScreen);
            SaveInventoryCommand = new RelayCommand(SaveInventory);
            LoadInventoryItems();
        }

        private async void LoadInventoryItems()
        {
            try
            {
                var inventoryItems = await _menuCoordinator.ReconstructInventoryItems();

                InventoryItems.Clear();
                foreach (var item in inventoryItems)
                {
                    InventoryItems.Add(item);
                }
                Log.Information("Loaded {Count} inventory items into the inventory editor view model", inventoryItems.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occured while trying to load the inventory items into the inventory editor viewmodel");
            }
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
                    await _inventoryService.ChangeInventoryItemQuantity(SelectedInventoryItem.InventoryItemId, SelectedInventoryItem.QuantityOnHand);
                }
                await _actionLogService.CreateActionLog(_navigationService.CurrentUser, "Modified Inventory", $"{_navigationService.CurrentUser.FirstName + " " + _navigationService.CurrentUser.LastName} modified existing inventory");
            }
        }
    }
}
