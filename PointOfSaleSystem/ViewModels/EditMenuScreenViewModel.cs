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
    public class EditMenuScreenViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        private readonly IMenuService _menuService;

        private readonly IInventoryMenuCoordinator _inventoryMenuCoordinator;

        private readonly IActionLogService _actionLogService;

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

        public EditMenuScreenViewModel(INavigationService navigationService, IMenuService menuService, IInventoryMenuCoordinator inventoryMenuCoordinator, IActionLogService actionLogService)
        {
            _navigationService = navigationService;
            _menuService = menuService;
            _inventoryMenuCoordinator = inventoryMenuCoordinator;
            _actionLogService = actionLogService;
            _menuItems = new ObservableCollection<MenuItem>(_menuService.LoadMenuItems());
            _categories = new ObservableCollection<string>
            {
                "Food",
                "Beverage",
                "Dessert",
                "Alcoholic",
                "Misc"
            };
            NavigateBackCommand = new RelayCommand(NavigateBack);
            SaveItemCommand = new RelayCommand(SaveItem);   
            SaveChangesCommand = new RelayCommand(SaveChanges);
        }

        public void NavigateBack()
        {
            _navigationService.Navigate<ManagerPanelScreenViewModel>();
        }

        public void SaveItem()
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
            _actionLogService.CreateActionLog(_navigationService.CurrentUser, "Created Menu Item", $"{_navigationService.CurrentUser.FirstName + " " + _navigationService.CurrentUser.LastName} created a new menu item named {NewItemName}");

            _menuItems = new ObservableCollection<MenuItem>(_menuService.LoadMenuItems());
            OnPropertyChanged(nameof(MenuItems));
        }

        public void SaveChanges()
        {
            if (SelectedMenuItem != null)
            {
                foreach (var item in MenuItems)
                {
                    SelectedMenuItem = item;
                    _menuService.UpdateItemPrice(SelectedMenuItem.ItemId, SelectedMenuItem.Price);
                    _menuService.UpdateItemName(SelectedMenuItem.ItemId, SelectedMenuItem.Name);
                }
                _actionLogService.CreateActionLog(_navigationService.CurrentUser, "Modified Menu", $"{_navigationService.CurrentUser.FirstName + " " + _navigationService.CurrentUser.LastName} modified existing menu items");
            }
        }
    }
}
