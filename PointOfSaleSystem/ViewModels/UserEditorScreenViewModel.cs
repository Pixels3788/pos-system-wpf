using PointOfSaleSystem.Services.Interfaces;
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

namespace PointOfSaleSystem.ViewModels
{
    public class UserEditorScreenViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        private readonly IUserService _userService;

        private ObservableCollection<User> _users;

        public ObservableCollection<User> Users
        {
            get => _users;
            set
            {
                SetProperty(ref _users, value);
            }
        }

        private User? _selectedUser;

        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
            }
        }

        public ICommand NavigateBackCommand { get; }

        public ICommand PromoteUserCommand { get; }

        public ICommand DemoteUserCommand { get; }

        public ICommand SaveChangesCommand { get; }

        public UserEditorScreenViewModel(INavigationService navigationService, IUserService userService)
        {
            _navigationService = navigationService;
            _userService = userService;
            _users = new ObservableCollection<User>(_userService.LoadUsers());
            NavigateBackCommand = new RelayCommand(NavigateBack);
            PromoteUserCommand = new RelayCommand(PromoteUser);
            DemoteUserCommand = new RelayCommand(DemoteUser);
            SaveChangesCommand = new RelayCommand(SaveChanges);
        }

        public void NavigateBack()
        {
            _navigationService.Navigate<ManagerPanelScreenViewModel>();
        }

        public void PromoteUser()
        {
            if (_selectedUser != null) {
                _userService.UpdateUserRole(SelectedUser.UserId, "Manager");
                Users = new ObservableCollection<User>(_userService.LoadUsers());
                
            }
        }

        public void DemoteUser()
        {
            if (_selectedUser != null)
            {
                _userService.UpdateUserRole(SelectedUser.UserId, "Associate");
                Users = new ObservableCollection<User>(_userService.LoadUsers());
            }
        }

        public void SaveChanges()
        {
            if (SelectedUser != null)
            {
                foreach (var user in _users) 
                {
                    SelectedUser = user;
                    _userService.UpdateUserPin(SelectedUser.UserId, SelectedUser.UserPin);
                    _userService.UpdateUserEmail(SelectedUser.UserId, SelectedUser.UserEmail);
                }
            }
        }
    }
}
