using PointOfSaleSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using PointOfSaleSystem.Helpers;
using PointOfSaleSystem.Models;
using PointOfSaleSystem.Services;
using System.Windows.Input;
using Serilog;


namespace PointOfSaleSystem.ViewModels
{
    public class ClosedOrdersScreenViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        private readonly IOrderService _orderService;

        private readonly IDialogService _dialogService;

        public decimal TodaysEarnings
        {
            get
            {
                decimal sum = 0m;
                foreach (var item in _closedOrders.Where(o => o.CreatedAt.Date == DateTime.Today))
                {
                    sum += item.TotalAfterTax;
                }
                return sum;
            }
           
        }

        public decimal LifeTimeEarnings
        {
            get 
            {
                decimal sum = 0m;
                foreach (var item in _closedOrders)
                {
                    sum += item.TotalAfterTax;
                }
                return sum;
            }
        }

        
        

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
        public ClosedOrdersScreenViewModel(INavigationService navigationService, IOrderService orderService, IDialogService dialogService) 
        {
            _navigationService = navigationService;
            _orderService = orderService;
            _dialogService = dialogService;
            _closedOrders = new ObservableCollection<Order>();
            NavigateToOpenOrdersCommand = new RelayCommand(NavigateToOpenOrders);
            LoadFinalizedOrders();
        }

        private async void LoadFinalizedOrders()
        {
            try
            {
                var finalizedOrders = await _orderService.GetFinalizedOrders();

                ClosedOrders.Clear();
                foreach (var order in finalizedOrders)
                {
                    ClosedOrders.Add(order);
                }
                Log.Information("Loaded {Count} finalized orders into the viewmodel", finalizedOrders.Count);
                
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading finalized orders into the viewmodel");
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
                Log.Error(ex, "Unexpected error occurred while attempting to navigate from the closed orders screen to the open orders screen");
                _dialogService.ShowError("Error occurred while navigating to the open orders screen", "Navigation Error");
            }
        }
    }
}
