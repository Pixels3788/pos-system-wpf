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

        public InventoryEditorScreenViewModel(IInventoryMenuCoordinator menuCoordinator, INavigationService navigationService, IInventoryService inventoryService)
        {
            _menuCoordinator = menuCoordinator;
            _navigationService = navigationService;
            _inventoryService = inventoryService;
            _inventoryItems = new ObservableCollection<InventoryItem>(_menuCoordinator.ReconstructInventoryItems());
            NavigateToOrderScreenCommand = new RelayCommand(NavigateToOrderScreen);
            SaveInventoryCommand = new RelayCommand(SaveInventory);
        }

        public void NavigateToOrderScreen()
        {
            _navigationService.Navigate<ManagerPanelScreenViewModel>();
        }

        public void SaveInventory()
        {
            if (SelectedInventoryItem != null)
            {
                foreach (var item in InventoryItems)
                {
                    _inventoryService.ChangeInventoryItemQuantity(SelectedInventoryItem.InventoryItemId, SelectedInventoryItem.QuantityOnHand);
                }
            }
        }
    }
}
