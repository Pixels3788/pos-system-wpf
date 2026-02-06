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

        BaseViewModel CurrentViewModel { get; }

        User? CurrentUser { get; }
    }
}
