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
using PointOfSaleSystem.Services.Interfaces;
using Serilog;

namespace PointOfSaleSystem.ViewModels
{
    public class OpenOrdersScreenViewModel : BaseViewModel
    {
        private readonly IOrderService _orderService;

        private readonly INavigationService _navigationService;

        private readonly IOrderInventoryCoordination _orderInventoryCoordination;

        private readonly IActionLogService _actionLogService;

        private ObservableCollection<Order> _openOrders;

        public ObservableCollection<Order> OpenOrders
        {
            get => _openOrders;
            set
            {
                SetProperty(ref _openOrders, value);
            }
        }

        public bool IsOrderSelected => SelectedOrder != null;

        private Order _selectedOrder;

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                
                SetProperty(ref _selectedOrder, value);
            }
        }

        private string _cashReceived;

        public string CashReceived
        {
            get => _cashReceived;
            set
            {
                SetProperty(ref _cashReceived, value);
            }
        }

        private decimal _lifetimeEarnings;

        public decimal LifetimeEarnings
        {
            get => _lifetimeEarnings;
            set
            {
                SetProperty(ref _lifetimeEarnings, value);
            }
        }

        private decimal _changeDue;

        public decimal ChangeDue
        {
            get => _changeDue;
            set
            {
                SetProperty(ref _changeDue, value);
            }
        }

        public ICommand NavigateToOrderScreenCommand { get; }

        public ICommand CancelOrderCommand { get; }

        public ICommand FinalizeOrderCommand { get; }

        public ICommand EditOrderCommand { get; }

        public ICommand NavigateToFinalizedOrdersCommand { get; }

        public OpenOrdersScreenViewModel(INavigationService navigationService, IOrderService orderService, IOrderInventoryCoordination orderInventoryCoordination, IActionLogService actionLogService)
        {
            _navigationService = navigationService;
            _orderService = orderService;
            _orderInventoryCoordination = orderInventoryCoordination;
            _actionLogService = actionLogService;
            _openOrders = new ObservableCollection<Order>();
            _selectedOrder = new Order();
            NavigateToOrderScreenCommand = new RelayCommand(NavigateToOrderScreen);
            CancelOrderCommand = new RelayCommand(CancelOrder);
            FinalizeOrderCommand = new RelayCommand(FinalizeOrder);
            EditOrderCommand = new RelayCommand(EditOrder);
            NavigateToFinalizedOrdersCommand = new RelayCommand(NavigateToFinalizedOrders);
            LoadOpenOrders();
            
        }

        private async void LoadOpenOrders()
        {
            try
            {
                var openOrders = await _orderService.GetOpenOrders();

                OpenOrders.Clear();
                foreach (var order in openOrders)
                {
                    OpenOrders.Add(order);
                }

                Log.Information("Loaded {Count} open orders in the viewmodel", openOrders.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading open orders in the viewmodel");
            }
        }

        public void NavigateToOrderScreen()
        {
            _navigationService.Navigate<OrderTakingScreenViewModel>();
        }

        public void NavigateToFinalizedOrders()
        {
            _navigationService.Navigate<ClosedOrdersScreenViewModel>();
        }
        public async void CancelOrder()
        {
            if (SelectedOrder != null)
            {
                foreach(var orderItem in SelectedOrder.LineOrder)
                {
                    await _orderInventoryCoordination.IncrementOnDeletion(orderItem);
                }
                await _actionLogService.CreateActionLog(_navigationService.CurrentUser, "Cancelled Order", $"{_navigationService.CurrentUser.FirstName + " " + _navigationService.CurrentUser.LastName} cancelled order {SelectedOrder.OrderId} which was worth a total of ${SelectedOrder.TotalAfterTax}");
                await _orderService.DeleteOrder(SelectedOrder.OrderId);
                OpenOrders.Remove(SelectedOrder);
            }
            else
            {
                return;
            }
        }

        public async void FinalizeOrder() 
        {
            if (_selectedOrder != null)
            {
                
                if (!decimal.TryParse(CashReceived, out decimal inputtedCash))
                {
                    return;
                }

                if (inputtedCash == SelectedOrder.TotalAfterTax || inputtedCash > SelectedOrder.TotalAfterTax)
                {
                    ChangeDue = inputtedCash - SelectedOrder.TotalAfterTax;
                    await _orderService.FinalizeOrder(SelectedOrder.OrderId);
                    await _actionLogService.CreateActionLog(_navigationService.CurrentUser, "Finalized Order", $"{_navigationService.CurrentUser.FirstName + " " + _navigationService.CurrentUser.LastName} finalized order {SelectedOrder.OrderId} by receiving ${inputtedCash} as payment and dispensing ${ChangeDue} in change back to the customer");
                    OpenOrders.Remove(SelectedOrder);
                    CashReceived = "";
                }
                else
                {
                    return;
                }
            }
        }

        public async void EditOrder()
        {
            await _actionLogService.CreateActionLog(_navigationService.CurrentUser, "Edited Order", $"{_navigationService.CurrentUser.FirstName + " " + _navigationService.CurrentUser.LastName} edited order {SelectedOrder.OrderId}");
            _navigationService.Navigate<OrderTakingScreenViewModel>(SelectedOrder);

        }
    }
}
