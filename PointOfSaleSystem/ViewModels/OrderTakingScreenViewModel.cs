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
                CurrentOrder = _orderService.CreateNewOrder();
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
            get
            {
                if (_currentOrder == null)
                {
                    _currentOrder = _orderService.CreateNewOrder();
                }
                return _currentOrder;
            }
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

        public OrderTakingScreenViewModel(IUserService userService, INavigationService navigationService, IOrderInventoryCoordination orderInventoryCoordination, IOrderService orderService, IMenuService menuService, IInventoryMenuCoordinator inventoryMenuCoordinator, IInventoryService inventoryService)
        {
            _userService = userService;
            _navigationService = navigationService;
            _orderInventoryCoordination = orderInventoryCoordination;
            _menuInventoryCoordination = inventoryMenuCoordinator;
            _orderService = orderService;
            _menuService = menuService;
            _inventoryService = inventoryService;
            _currentOrderLineItems = new ObservableCollection<OrderLineItem>();
            _menuItems = new ObservableCollection<MenuItem>(_menuService.LoadMenuItems());
            foreach (var item in _menuItems)
            {
                var inventoryItem = _inventoryService.GetInventoryItemByMenuItemId(item.ItemId);

                if (inventoryItem != null)
                {
                    item.IsAvailable = inventoryItem.IsAvailable;
                }
            }
            
       
            
            AddOrderLineItem = new RelayCommand<MenuItem>(AddNewOrderLineItemToOrder);
            SeedMenuItemsCommand = new RelayCommand(SeedMenuItems);
            _currentOrderLineItems.CollectionChanged += (s, e) => OnPropertyChanged(nameof(OrderTotal));
            SendOrderCommand = new RelayCommand(SendOrder);
            CancelOrderCommand = new RelayCommand(CancelOrder); 
            DeleteOrderItemCommand = new RelayCommand(DeleteOrderLineItemFromOrder);
            LogoutCommand = new RelayCommand(Logout);
            NavigateToOpenOrdersCommand = new RelayCommand(NavigateToOpenOrders);
            NavigateToManagerPanelCommand = new RelayCommand(NavigateToManagerPanel);
            
        }

        public void AddNewOrderLineItemToOrder(MenuItem menuItem) 
        {
            bool updated = false;

            foreach(var item in _currentOrderLineItems)
            {
                if (item.MenuItemId == menuItem.ItemId)
                {
                    _orderInventoryCoordination.DecrementOnQuantityChanged(item, 1);
                    _orderService.UpdateOrderLineItemQuantity(item.LineItemId, item.Quantity + 1);
                    
                    var inventoryItem = _inventoryService.GetInventoryItemByMenuItemId(item.MenuItemId);
                    if ( inventoryItem != null)
                    {
                        menuItem.IsAvailable = inventoryItem.IsAvailable;
                    }

                    CurrentOrderLineItems = new ObservableCollection<OrderLineItem>(_orderService.GetOrderLineItemsByOrder(CurrentOrder.OrderId));
                    OnPropertyChanged(nameof(OrderTotal));
                    updated = true;
                    break;
                }
            }
            if (!updated) 
            {
                OrderLineItem? newOrderItem = _orderInventoryCoordination.DecrementOnCreation(menuItem, CurrentOrder, 1);
                if (newOrderItem != null) 
                {
                    CurrentOrderLineItems.Add(newOrderItem);
                    var inventoryItem = _inventoryService.GetInventoryItemByMenuItemId(newOrderItem.MenuItemId);
                    if (inventoryItem != null)
                    {
                        menuItem.IsAvailable = inventoryItem.IsAvailable;
                    }
                    OnPropertyChanged(nameof(OrderTotal));
                }
            }
        }

        public void DeleteOrderLineItemFromOrder()
        {
            
            _orderInventoryCoordination.IncrementOnDeletion(SelectedOrderItem);
            var menuItem = MenuItems.FirstOrDefault(m => m.ItemId == SelectedOrderItem.MenuItemId);
            if (menuItem != null)
            {
                var inventoryItem = _inventoryService.GetInventoryItemByMenuItemId(menuItem.ItemId);
                if (inventoryItem != null) 
                {
                    menuItem.IsAvailable = inventoryItem.IsAvailable;
                }
            }


            CurrentOrderLineItems.Remove(SelectedOrderItem);
            OnPropertyChanged(nameof(OrderTotal));
        }

        public void SeedMenuItems()
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
            _userService.CreateUser("Manager", "Test", "manager", 4323);
            var manager = _userService.GetUserByPin(4323);
            _userService.UpdateUserRole(manager.UserId, "Manager");

        }

        public void Logout()
        {
            if (CurrentOrderLineItems.Count <= 0)
            {
                _orderService.DeleteOrder(CurrentOrder.OrderId);
            }
            
            _navigationService.Navigate<LoginScreenViewModel>();
            
        }

        public void SendOrder()
        {
            CurrentOrder = _orderService.CreateNewOrder();
            CurrentOrderLineItems.Clear();
            OnPropertyChanged(nameof(OrderTotal));
        }

        public void CancelOrder()
        {
            foreach (var item in CurrentOrderLineItems) 
            {
                _orderInventoryCoordination.IncrementOnDeletion(item);

                var menuItem = MenuItems.FirstOrDefault(m => m.ItemId == item.MenuItemId);
                if (menuItem != null) 
                {
                    var inventoryItem = _inventoryService.GetInventoryItemByMenuItemId(menuItem.ItemId);

                    if (inventoryItem != null)
                    {
                        menuItem.IsAvailable = inventoryItem.IsAvailable;
                    }
                }
            }

            _orderService.DeleteOrder(CurrentOrder.OrderId);
            CurrentOrder = _orderService.CreateNewOrder();
            CurrentOrderLineItems.Clear();
            OnPropertyChanged(nameof(OrderTotal));
        }

        public void NavigateToOpenOrders()
        {
            if (CurrentOrder != null && CurrentOrderLineItems.Count <= 0)
            {
                _orderService.DeleteOrder(CurrentOrder.OrderId);
            }
            _navigationService.Navigate<OpenOrdersScreenViewModel>();
        }

        public void NavigateToManagerPanel()
        {
            _navigationService.Navigate<ManagerPanelScreenViewModel>();  
        }


    }
}
