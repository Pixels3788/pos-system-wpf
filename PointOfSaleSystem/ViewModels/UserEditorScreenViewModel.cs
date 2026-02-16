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
using Serilog;

namespace PointOfSaleSystem.ViewModels
{
    public class UserEditorScreenViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        private readonly IUserService _userService;

        private readonly IActionLogService _actionLogService;

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

        public ICommand DeleteUserCommand { get; }

        public UserEditorScreenViewModel(INavigationService navigationService, IUserService userService, IActionLogService actionLogService)
        {
            _navigationService = navigationService;
            _userService = userService;
            _actionLogService = actionLogService;
            _users = new ObservableCollection<User>();
            NavigateBackCommand = new RelayCommand(NavigateBack);
            PromoteUserCommand = new RelayCommand(PromoteUser);
            DemoteUserCommand = new RelayCommand(DemoteUser);
            SaveChangesCommand = new RelayCommand(SaveChanges);
            DeleteUserCommand = new RelayCommand(DeleteUser);
            LoadUsers();
        }

        public async void LoadUsers()
        {
            try
            {
                var users = await _userService.LoadUsers();

                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }
                Log.Information("Loaded {Count} users in the viewmodel", users.Count);
            }
            catch (Exception ex) 
            {
                Log.Error(ex, "Error loading users in viewmodel");
            }
        }
        public void NavigateBack()
        {
            _navigationService.Navigate<ManagerPanelScreenViewModel>();
        }

        public async void PromoteUser()
        {
            if (_selectedUser != null) {
                await _actionLogService.CreateActionLog(_navigationService.CurrentUser, "Promotion", $"{_navigationService.CurrentUser.FirstName + " " + _navigationService.CurrentUser.LastName} promoted {SelectedUser.FirstName + " " + SelectedUser.LastName} to manager");
                await _userService.UpdateUserRole(SelectedUser.UserId, "Manager");
                LoadUsers();
                
            }
        }

        public async void DemoteUser()
        {
            if (_selectedUser != null)
            {
                await _actionLogService.CreateActionLog(_navigationService.CurrentUser, "Demotion", $"{_navigationService.CurrentUser.FirstName + " " + _navigationService.CurrentUser.LastName} demoted {SelectedUser.FirstName + " " + SelectedUser.LastName} from manager to associate");
                await _userService.UpdateUserRole(SelectedUser.UserId, "Associate");
                LoadUsers();
            }
        }

        public async void SaveChanges()
        {
            if (SelectedUser != null)
            {
                foreach (var user in _users) 
                {
                    SelectedUser = user;
                    await _userService.UpdateUserPin(SelectedUser.UserId, SelectedUser.UserPin);
                    await _userService.UpdateUserEmail(SelectedUser.UserId, SelectedUser.UserEmail);
                }
                await _actionLogService.CreateActionLog(_navigationService.CurrentUser, "User Information Modification", $"{_navigationService.CurrentUser.FirstName + " " + _navigationService.CurrentUser.LastName} modified user information");

            }
        }

        public async void DeleteUser()
        {
            if (_selectedUser != null)
            {
                await _userService.DeleteUser(SelectedUser.UserId);
                Users.Remove(SelectedUser);
            }
        }
    }
}
