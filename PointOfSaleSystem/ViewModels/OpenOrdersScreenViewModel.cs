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

namespace PointOfSaleSystem.ViewModels
{
    public class OpenOrdersScreenViewModel : BaseViewModel
    {
        private readonly IOrderService _orderService;

        private readonly INavigationService _navigationService;

        private readonly IOrderInventoryCoordination _orderInventoryCoordination;

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

        public ICommand NavigateToOrderScreenCommand { get; }

        public ICommand CancelOrderCommand { get; }

        public ICommand FinalizeOrderCommand { get; }

        public ICommand EditOrderCommand { get; }

        public ICommand NavigateToFinalizedOrdersCommand { get; }

        public OpenOrdersScreenViewModel(INavigationService navigationService, IOrderService orderService, IOrderInventoryCoordination orderInventoryCoordination)
        {
            _navigationService = navigationService;
            _orderService = orderService;
            _orderInventoryCoordination = orderInventoryCoordination;
            _openOrders = new ObservableCollection<Order>(_orderService.GetOpenOrders());
            _selectedOrder = new Order();
            NavigateToOrderScreenCommand = new RelayCommand(NavigateToOrderScreen);
            CancelOrderCommand = new RelayCommand(CancelOrder);
            FinalizeOrderCommand = new RelayCommand(FinalizeOrder);
            EditOrderCommand = new RelayCommand(EditOrder);
            NavigateToFinalizedOrdersCommand = new RelayCommand(NavigateToFinalizedOrders);
        }

        public void NavigateToOrderScreen()
        {
            _navigationService.Navigate<OrderTakingScreenViewModel>();
        }

        public void NavigateToFinalizedOrders()
        {
            _navigationService.Navigate<ClosedOrdersScreenViewModel>();
        }
        public void CancelOrder()
        {
            if (SelectedOrder != null)
            {
                foreach(var orderItem in SelectedOrder.LineOrder)
                {
                    _orderInventoryCoordination.IncrementOnDeletion(orderItem);
                }
                _orderService.DeleteOrder(SelectedOrder.OrderId);
                OpenOrders.Remove(SelectedOrder);
            }
            else
            {
                return;
            }
        }

        public void FinalizeOrder() 
        {
            if (_selectedOrder != null)
            {
                
                if (!decimal.TryParse(CashReceived, out decimal inputtedCash))
                {
                    return;
                }

                if (inputtedCash == SelectedOrder.OrderTotal)
                {
                    _orderService.FinalizeOrder(SelectedOrder.OrderId);
                    OpenOrders.Remove(SelectedOrder);
                    CashReceived = "";
                }
                else
                {
                    return;
                }
            }
        }

        public void EditOrder()
        {
            _navigationService.Navigate<OrderTakingScreenViewModel>(SelectedOrder);

        }
    }
}
