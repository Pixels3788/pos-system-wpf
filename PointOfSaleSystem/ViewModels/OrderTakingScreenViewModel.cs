using PointOfSaleSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using PointOfSaleSystem.Helpers;
using PointOfSaleSystem.Models;
using PointOfSaleSystem.Services;
using System.Windows.Input;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using Serilog;

// View model for the order taking screen
namespace PointOfSaleSystem.ViewModels
{
    public class OrderTakingScreenViewModel : BaseViewModel, INavigable
    {

        public void Initialize(object parameter)
        {
            if (parameter is Order order)
            {
                CurrentOrder = order;
                foreach(var OrderItem in CurrentOrder.LineOrder)
                {
                    CurrentOrderLineItems.Add(OrderItem);
                }
            } 
            else
            {
                
                CurrentOrderLineItems = new ObservableCollection<OrderLineItem>();
            }

            OnPropertyChanged(nameof(CurrentOrder));
            OnPropertyChanged(nameof(CurrentOrderLineItems));
            OnPropertyChanged(nameof(OrderTotal));
        }

        private readonly INavigationService _navigationService;

        private readonly IOrderInventoryCoordination _orderInventoryCoordination;

        private readonly IInventoryMenuCoordinator _menuInventoryCoordination;

        private readonly IOrderService _orderService;

        private readonly IMenuService _menuService;

        private readonly IInventoryService _inventoryService;

        private readonly IUserService _userService;

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

        private ObservableCollection<OrderLineItem> _currentOrderLineItems;

        public ObservableCollection<OrderLineItem> CurrentOrderLineItems
        {
            get => _currentOrderLineItems;
            set
            {
                SetProperty(ref _currentOrderLineItems, value);
            }
        }

        

        public bool IsManager
        {
            get
            {
                if (_navigationService.CurrentUser.UserRole == "Manager")
                {
                    return true;
                }
                else
                {
                    return false; 
                }
            }
        }
        

        

        public decimal OrderTotal
        {
            get {
                decimal sum = 0m;
                foreach(var item in _currentOrderLineItems) 
                {
                  sum += item.UnitPrice * item.Quantity;
                  
                }
                return sum;
           }
        }

        public decimal TotalAfterTax
        {
            get => OrderTotal * 1.08m;
        }
        

        private OrderLineItem _selectedOrderItem;

        public OrderLineItem SelectedOrderItem
        {
            get => _selectedOrderItem;
            set
            {
                SetProperty(ref _selectedOrderItem, value);
            }
        }




        private Order? _currentOrder;

        public Order? CurrentOrder
        {
            get => _currentOrder;
            set
            {
                SetProperty(ref _currentOrder, value);
            }
        }

        public ICommand AddOrderLineItem { get; }

        public ICommand SendOrderCommand { get; }

        public ICommand CancelOrderCommand { get; }

        public ICommand DeleteOrderItemCommand { get; }

        public ICommand SeedMenuItemsCommand { get; }

        public ICommand LogoutCommand { get; }

        public ICommand NavigateToOpenOrdersCommand { get; }

        public ICommand NavigateToManagerPanelCommand { get; }

        public OrderTakingScreenViewModel(IUserService userService, INavigationService navigationService, IOrderInventoryCoordination orderInventoryCoordination, IOrderService orderService, IMenuService menuService, IInventoryMenuCoordinator inventoryMenuCoordinator, IInventoryService inventoryService, IActionLogService actionLogService, IDialogService dialogService)
        {
            _userService = userService;
            _navigationService = navigationService;
            _orderInventoryCoordination = orderInventoryCoordination;
            _menuInventoryCoordination = inventoryMenuCoordinator;
            _orderService = orderService;
            _menuService = menuService;
            _inventoryService = inventoryService;
            _actionLogService = actionLogService;
            _dialogService = dialogService;
            _currentOrderLineItems = new ObservableCollection<OrderLineItem>();
            _menuItems = new ObservableCollection<MenuItem>();
            LoadMenuItems();
            
            
       
            
            AddOrderLineItem = new AsyncRelayCommand<MenuItem>(AddNewOrderLineItemToOrder);
            SeedMenuItemsCommand = new AsyncRelayCommand(SeedMenuItems);
            _currentOrderLineItems.CollectionChanged += (s, e) => OnPropertyChanged(nameof(OrderTotal));
            SendOrderCommand = new AsyncRelayCommand(SendOrder);
            CancelOrderCommand = new AsyncRelayCommand(CancelOrder, () => CurrentOrder != null); 
            DeleteOrderItemCommand = new AsyncRelayCommand(DeleteOrderLineItemFromOrder);
            LogoutCommand = new AsyncRelayCommand(Logout);
            NavigateToOpenOrdersCommand = new RelayCommand(NavigateToOpenOrders);
            NavigateToManagerPanelCommand = new AsyncRelayCommand(NavigateToManagerPanel);
            
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
                foreach (var item in _menuItems)
                {
                    var inventoryItem = await _inventoryService.GetInventoryItemByMenuItemId(item.ItemId);

                    if (inventoryItem != null)
                    {
                        item.IsAvailable = inventoryItem.IsAvailable;
                    }
                }
                Log.Information("Loaded {Count} menu items into the viewmodel", menuItems.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading menu items in the viewmodel");
            }
        }

        public async Task AddNewOrderLineItemToOrder(MenuItem menuItem) 
        {
            try
            {
                if (CurrentOrder == null)
                {
                    CurrentOrder = await _orderService.CreateNewOrder();
                }

                bool updated = false;

                foreach (var item in _currentOrderLineItems)
                {
                    if (item.MenuItemId == menuItem.ItemId)
                    {
                        await _orderInventoryCoordination.DecrementOnQuantityChanged(item, 1);
                        await _orderService.UpdateOrderLineItemQuantity(item.LineItemId, item.Quantity + 1);

                        var inventoryItem = await _inventoryService.GetInventoryItemByMenuItemId(item.MenuItemId);
                        if (inventoryItem != null)
                        {
                            menuItem.IsAvailable = inventoryItem.IsAvailable;
                        }

                        CurrentOrderLineItems = new ObservableCollection<OrderLineItem>(await _orderService.GetOrderLineItemsByOrder(CurrentOrder.OrderId));
                        OnPropertyChanged(nameof(OrderTotal));
                        OnPropertyChanged(nameof(TotalAfterTax));
                        updated = true;
                        break;
                    }
                }
                if (!updated)
                {
                    OrderLineItem? newOrderItem = await _orderInventoryCoordination.DecrementOnCreation(menuItem, CurrentOrder, 1);
                    if (newOrderItem != null)
                    {
                        CurrentOrderLineItems.Add(newOrderItem);
                        var inventoryItem = await _inventoryService.GetInventoryItemByMenuItemId(newOrderItem.MenuItemId);
                        if (inventoryItem != null)
                        {
                            menuItem.IsAvailable = inventoryItem.IsAvailable;
                        }
                        OnPropertyChanged(nameof(OrderTotal));
                        OnPropertyChanged(nameof(TotalAfterTax));
                    }
                }
            }
            catch(Exception ex) 
            {
                Log.Error(ex, "Unexpected error while attempting to add an order line item to an order inside the order taking viewmodel");
                _dialogService.ShowError("Error occurred while trying to add item to order", "Adding Item Error");
            }
        }

        public async Task DeleteOrderLineItemFromOrder()
        {
            try
            {
                await _orderInventoryCoordination.IncrementOnDeletion(SelectedOrderItem);
                var menuItem = MenuItems.FirstOrDefault(m => m.ItemId == SelectedOrderItem.MenuItemId);
                if (menuItem != null)
                {
                    var inventoryItem = await _inventoryService.GetInventoryItemByMenuItemId(menuItem.ItemId);
                    if (inventoryItem != null)
                    {
                        menuItem.IsAvailable = inventoryItem.IsAvailable;
                    }
                }


                CurrentOrderLineItems.Remove(SelectedOrderItem);
                OnPropertyChanged(nameof(OrderTotal));
                OnPropertyChanged(nameof(TotalAfterTax));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while attempting to delete a line item from an order inside the order taking viewmodel");
                _dialogService.ShowError("Error occurred while trying to delete the item from the order", "Line Item Deletion Error");
            }
        }

        public async Task SeedMenuItems()
        {
            _menuInventoryCoordination.CreateInventoryForMenuItem("Classic Burger", 5.99m, "Food", 50);
            _menuInventoryCoordination.CreateInventoryForMenuItem("Cheeseburger", 6.49m, "Food", 50);
            _menuInventoryCoordination.CreateInventoryForMenuItem("Bacon Burger", 7.49m, "Food", 40);
            _menuInventoryCoordination.CreateInventoryForMenuItem("Double Burger", 8.99m, "Food", 35);
            _menuInventoryCoordination.CreateInventoryForMenuItem("Mushroom Swiss Burger", 7.99m, "Food", 30);
            _menuInventoryCoordination.CreateInventoryForMenuItem("BBQ Burger", 7.79m, "Food", 30);

            // Chicken
            _menuInventoryCoordination.CreateInventoryForMenuItem("Grilled Chicken Sandwich", 6.99m, "Food", 40);
            _menuInventoryCoordination.CreateInventoryForMenuItem("Crispy Chicken Sandwich", 6.79m, "Food", 40);
            _menuInventoryCoordination.CreateInventoryForMenuItem("Chicken Tenders (3pc)", 5.49m, "Food", 60);
            _menuInventoryCoordination.CreateInventoryForMenuItem("Chicken Wrap", 6.25m, "Food", 35);

            // Sides
            _menuInventoryCoordination.CreateInventoryForMenuItem("Small Fries", 2.49m, "Side", 100);
            _menuInventoryCoordination.CreateInventoryForMenuItem("Medium Fries", 2.99m, "Side", 80);
            _menuInventoryCoordination.CreateInventoryForMenuItem("Large Fries", 3.49m, "Side", 70);
            _menuInventoryCoordination.CreateInventoryForMenuItem("Onion Rings", 3.99m, "Side", 60);
            _menuInventoryCoordination.CreateInventoryForMenuItem("Mozzarella Sticks", 4.49m, "Side", 50);

            // Beverages
            _menuInventoryCoordination.CreateInventoryForMenuItem("Coca-Cola", 2.19m, "Beverage", 150);
            _menuInventoryCoordination.CreateInventoryForMenuItem("Diet Coke", 2.19m, "Beverage", 120);
            _menuInventoryCoordination.CreateInventoryForMenuItem("Sprite", 2.19m, "Beverage", 120);
            _menuInventoryCoordination.CreateInventoryForMenuItem("Dr Pepper", 2.19m, "Beverage", 100);
            _menuInventoryCoordination.CreateInventoryForMenuItem("Iced Tea", 2.49m, "Beverage", 80);
            _menuInventoryCoordination.CreateInventoryForMenuItem("Lemonade", 2.49m, "Beverage", 80);
            _menuInventoryCoordination.CreateInventoryForMenuItem("Bottled Water", 1.99m, "Beverage", 200);

            // Desserts
            _menuInventoryCoordination.CreateInventoryForMenuItem("Chocolate Milkshake", 3.99m, "Dessert", 40);
            _menuInventoryCoordination.CreateInventoryForMenuItem("Vanilla Milkshake", 3.99m, "Dessert", 1);
            await _userService.CreateUser("Manager", "Test", "manager", 4323);
            var manager = await _userService.GetUserByPin(4323);
            await _userService.UpdateUserRole(manager.UserId, "Manager");

        }

        public async Task Logout()
        {
            try
            {
                await _actionLogService.CreateActionLog(_navigationService.CurrentUser, "Logged Out", $"{_navigationService.CurrentUser.FirstName} {_navigationService.CurrentUser.LastName} Logged Out of the POS");
                _navigationService.Navigate<LoginScreenViewModel>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred while trying to logout inside of the order taking view model");
                _dialogService.ShowError("Error occurred while trying to logout of the POS", "Logout Error");
            }
        }

        public async Task SendOrder()
        {
            try
            {
                if (CurrentOrder == null || CurrentOrderLineItems.Count <= 0) return;
                await _actionLogService.CreateActionLog(_navigationService.CurrentUser, "Order Creation", $"{_navigationService.CurrentUser.FirstName + " " + _navigationService.CurrentUser.LastName} created a new order with the ID {CurrentOrder.OrderId} worth a total of ${Math.Round(TotalAfterTax, 2)}");
                CurrentOrder = null;
                CurrentOrderLineItems.Clear();
                OnPropertyChanged(nameof(OrderTotal));
                OnPropertyChanged(nameof(TotalAfterTax));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred while trying to send an order inside of the order taking view model");
                _dialogService.ShowError("Error occurred while attempting to send the current order through", "Sent Order Error");
            }
        }

        public async Task CancelOrder()
        {
            try
            {
                foreach (var item in CurrentOrderLineItems)
                {
                    await _orderInventoryCoordination.IncrementOnDeletion(item);

                    var menuItem = MenuItems.FirstOrDefault(m => m.ItemId == item.MenuItemId);
                    if (menuItem != null)
                    {
                        var inventoryItem = await _inventoryService.GetInventoryItemByMenuItemId(menuItem.ItemId);

                        if (inventoryItem != null)
                        {
                            menuItem.IsAvailable = inventoryItem.IsAvailable;
                        }
                    }
                }
                await _actionLogService.CreateActionLog(_navigationService.CurrentUser, "Deleted Order", $"{_navigationService.CurrentUser.FirstName + " " + _navigationService.CurrentUser.LastName} cancelled order {CurrentOrder.OrderId} Which was worth ${Math.Round(TotalAfterTax, 2)}");

                await _orderService.DeleteOrder(CurrentOrder.OrderId);
                CurrentOrder = null;
                CurrentOrderLineItems.Clear();
                OnPropertyChanged(nameof(OrderTotal));
                OnPropertyChanged(nameof(TotalAfterTax));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred while trying to cancel an order inside the order taking view model");
                _dialogService.ShowError("Error occurred while attempting to cancel the current order", "Order Cancellation Error");
            }
        }

        public void NavigateToOpenOrders()
        {
            try
            {
                _navigationService.Navigate<OpenOrdersScreenViewModel>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred while attempting to navigate to the open orders screen from the order taking screen");
                _dialogService.ShowError("Error occurred while trying to navigate to the open orders screen, please try again", "Navigation Error");
            }
        }

        public async Task NavigateToManagerPanel()
        {
            try
            {
                await _actionLogService.CreateActionLog(_navigationService.CurrentUser, "Accessed Manager Panel", $"{_navigationService.CurrentUser.FirstName + " " + _navigationService.CurrentUser.LastName} Accessed the manager panel");
                _navigationService.Navigate<ManagerPanelScreenViewModel>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred while attempting to navigate to the manager panel from the order taking screen");
                _dialogService.ShowError("Error occurred while trying to navigate to the manager panel, please try again", "Navigation Error");
            }
        }


    }
}
