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
    public class EditMenuScreenViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        private readonly IMenuService _menuService;

        private readonly IInventoryService _inventoryService;

        private readonly IInventoryMenuCoordinator _inventoryMenuCoordinator;

        private readonly IActionLogService _actionLogService;

        private readonly IDialogService _dialogService;

        private ObservableCollection<MenuItem> _menuItems;

        public ObservableCollection<MenuItem> MenuItems
        {
            get => _menuItems;
            set
            {
                SetProperty(ref _menuItems, value);
            }
        }

        private ObservableCollection<string> _categories;

        public ObservableCollection<string> Categories
        {
            get => _categories;
            set
            {
                SetProperty(ref _categories, value);
            }
        }

        private string? _newItemName;

        public string? NewItemName
        {
            get => _newItemName;
            set
            {
                SetProperty(ref _newItemName, value);
            }
        }

        private string? _newItemPrice;

        public string? NewItemPrice
        {
            get => _newItemPrice;
            set
            {
                SetProperty(ref _newItemPrice, value);
            }
        }

        private string? _newItemCategory;

        public string? NewItemCategory
        {
            get => _newItemCategory;
            set
            {
                SetProperty(ref _newItemCategory, value);   
            }
        }

        private string? _newItemQuantity;

        public string? NewItemQuantity
        {
            get => _newItemQuantity;
            set
            {
                SetProperty(ref _newItemQuantity, value);   
            }
        }

        private MenuItem? _selectedMenuItem;

        public MenuItem? SelectedMenuItem
        {
            get => _selectedMenuItem;
            set
            {
                SetProperty(ref _selectedMenuItem, value);
            }
        }

        public ICommand NavigateBackCommand { get; }

        public ICommand SaveItemCommand { get; }

        public ICommand SaveChangesCommand { get; }

        public ICommand DeleteMenuItemCommand { get; }

        public EditMenuScreenViewModel(INavigationService navigationService, IMenuService menuService, IInventoryMenuCoordinator inventoryMenuCoordinator, IActionLogService actionLogService, IInventoryService inventoryService, IDialogService dialogService)
        {
            _navigationService = navigationService;
            _menuService = menuService;
            _inventoryService = inventoryService;
            _inventoryMenuCoordinator = inventoryMenuCoordinator;
            _actionLogService = actionLogService;
            _dialogService = dialogService;
            _menuItems = new ObservableCollection<MenuItem>();
            _categories = new ObservableCollection<string>
            {
                "Food",
                "Beverage",
                "Dessert",
                "Alcoholic",
                "Misc"
            };
            NavigateBackCommand = new RelayCommand(NavigateBack);
            SaveItemCommand = new AsyncRelayCommand(SaveItem);   
            SaveChangesCommand = new AsyncRelayCommand(SaveChanges);
            DeleteMenuItemCommand = new AsyncRelayCommand(DeleteMenuItem);
            LoadMenuItems();
        }

        private async void LoadMenuItems()
        {
            try
            {
                var menuItems = await _menuService.LoadMenuItems();

                MenuItems.Clear();
                foreach (var item in menuItems)
                {
                    MenuItems.Add(item);
                }
                Log.Information("Loaded {Count} Menu items into the menu editor view model", menuItems.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading menu items into the menu editor view model");
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
                Log.Error(ex, "Unexpected error occurred while navigating to the manager panel from the edit menu screen");
                _dialogService.ShowError("Error, could not navigate to the manager panel", "Navigation Error");
            }
        }

        public async Task SaveItem()
        {
            try
            {
                if (!int.TryParse(_newItemQuantity, out int newItemQuantity))
                {
                    return;
                }

                if (!decimal.TryParse(_newItemPrice, out decimal newItemPrice))
                {
                    return;
                }

                if (newItemQuantity < 0) return;

                if (string.IsNullOrWhiteSpace(NewItemName)) return;

                if (string.IsNullOrWhiteSpace(NewItemCategory)) return;

                _inventoryMenuCoordinator.CreateInventoryForMenuItem(NewItemName, newItemPrice, NewItemCategory, newItemQuantity);
                await _actionLogService.CreateActionLog(_navigationService.CurrentUser, "Created Menu Item", $"{_navigationService.CurrentUser.FirstName + " " + _navigationService.CurrentUser.LastName} created a new menu item named {NewItemName}");

                LoadMenuItems();
                OnPropertyChanged(nameof(MenuItems));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred while saving a new item inside the edit menu screen");
                _dialogService.ShowError("Error, could not save the new item, please try again", "Item Saving Error");
            }
        }

        public async Task SaveChanges()
        {
            try
            {
                if (SelectedMenuItem != null)
                {
                    foreach (var item in MenuItems)
                    {
                        SelectedMenuItem = item;
                        await _menuService.UpdateItemPrice(SelectedMenuItem.ItemId, SelectedMenuItem.Price);
                        await _menuService.UpdateItemName(SelectedMenuItem.ItemId, SelectedMenuItem.Name);
                    }
                    await _actionLogService.CreateActionLog(_navigationService.CurrentUser, "Modified Menu", $"{_navigationService.CurrentUser.FirstName + " " + _navigationService.CurrentUser.LastName} modified existing menu items");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred while saving changes to the menu items inside the edit menu screen");
                _dialogService.ShowError("Error, could not save the changes to the menu items, please try again", "Save Changes Error");
            }
        }

        public async Task DeleteMenuItem()
        {
            try
            {
                if (SelectedMenuItem != null)
                {
                    await _menuService.DeleteMenuItem(SelectedMenuItem.ItemId);
                    _menuItems.Remove(SelectedMenuItem);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred while deleting a menu item inside the edit menu screen");
                _dialogService.ShowError("Error, could not delete item, please try again", "Item Deletion Error");
            }
        }
    }
}
