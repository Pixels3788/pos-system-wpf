using PointOfSaleSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using PointOfSaleSystem.Helpers;
using PointOfSaleSystem.Models;
using PointOfSaleSystem.Services;
using System.Windows.Input;


namespace PointOfSaleSystem.ViewModels
{
    public class ClosedOrdersScreenViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        private readonly IOrderService _orderService;

        private ObservableCollection<Order> _closedOrders;

        public ObservableCollection<Order> ClosedOrders
        {
            get => _closedOrders;
            set
            {
                SetProperty(ref _closedOrders, value);
            }
        }

        private Order? _selectedOrder;

        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                SetProperty(ref _selectedOrder, value);
            }
        }

        public ICommand NavigateToOpenOrdersCommand { get; }
        public ClosedOrdersScreenViewModel(INavigationService navigationService, IOrderService orderService) 
        {
            _navigationService = navigationService;
            _orderService = orderService;
            _closedOrders = new ObservableCollection<Order>(_orderService.GetFinalizedOrders());
            NavigateToOpenOrdersCommand = new RelayCommand(NavigateToOpenOrders);
        }

        public void NavigateToOpenOrders() 
        {
            _navigationService.Navigate<OpenOrdersScreenViewModel>();
        }
    }
}
