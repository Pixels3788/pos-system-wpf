using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using PointOfSaleSystem.Models;
using PointOfSaleSystem.ViewModels;

namespace PointOfSaleSystem.Services.Interfaces
{
    public interface INavigationService : INotifyPropertyChanged
    {
        void SetCurrentUser(User user);

        BaseViewModel CurrentViewModel { get; set; }

        User? CurrentUser { get; }
        public void Navigate<TViewModel>() where TViewModel : BaseViewModel;
        void Navigate<TViewModel>(object? parameter) where TViewModel : BaseViewModel;

        public void Register<TViewModel>(Func<TViewModel> factory) where TViewModel : BaseViewModel;

    }
}
