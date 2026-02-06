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

        public BaseViewModel? CurrentViewModel => _navigationService.CurrentViewModel;

        public MainWindowViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            _navigationService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(INavigationService.CurrentViewModel))
                    OnPropertyChanged(nameof(CurrentViewModel));
            };
        }
    }
}
