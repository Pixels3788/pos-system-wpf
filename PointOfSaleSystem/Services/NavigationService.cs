using PointOfSaleSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using PointOfSaleSystem.ViewModels;
using PointOfSaleSystem.Models;

namespace PointOfSaleSystem.Services
{
    public class NavigationService : INavigationService, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private BaseViewModel _currentViewModel;
        private User? _currentUser;

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    OnPropertyChanged(nameof(CurrentViewModel));
                }
            }
        }

        public User? CurrentUser
        {
            get => _currentUser;
            private set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    OnPropertyChanged(nameof(CurrentUser));
                }
            }
        }

        public void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

       

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
