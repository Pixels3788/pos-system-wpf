using PointOfSaleSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using PointOfSaleSystem.Services;
using PointOfSaleSystem.Services.Interfaces;

namespace PointOfSaleSystem.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        private BaseViewModel? _currentViewModel;
        public BaseViewModel? CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public MainWindowViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            CurrentViewModel = _navigationService.CurrentViewModel;

            _navigationService.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(INavigationService.CurrentViewModel))
                    CurrentViewModel = _navigationService.CurrentViewModel;
            };
        }
    }
}
