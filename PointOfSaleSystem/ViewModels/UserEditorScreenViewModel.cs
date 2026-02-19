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

// View model for the user editor screen
namespace PointOfSaleSystem.ViewModels
{
    public class UserEditorScreenViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        private readonly IUserService _userService;

        private readonly IActionLogService _actionLogService;

        private readonly IDialogService _dialogService;

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

        public UserEditorScreenViewModel(INavigationService navigationService, IUserService userService, IActionLogService actionLogService, IDialogService dialogService)
        {
            _navigationService = navigationService;
            _userService = userService;
            _actionLogService = actionLogService;
            _dialogService = dialogService;
            _users = new ObservableCollection<User>();
            NavigateBackCommand = new RelayCommand(NavigateBack);
            PromoteUserCommand = new AsyncRelayCommand(PromoteUser);
            DemoteUserCommand = new AsyncRelayCommand(DemoteUser);
            SaveChangesCommand = new AsyncRelayCommand(SaveChanges);
            DeleteUserCommand = new AsyncRelayCommand(DeleteUser);
            LoadUsers();
            _dialogService = dialogService;
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
            try
            {
                _navigationService.Navigate<ManagerPanelScreenViewModel>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred while attempting to navigate to the manager panel from the user editor");
                _dialogService.ShowError("Error: An error occurred while attempting to navigate to the manager panel, please try again", "Navigation Error");
            }
        }

        public async Task PromoteUser()
        {
            try
            {
                if (_selectedUser != null)
                {
                    await _actionLogService.CreateActionLog(_navigationService.CurrentUser, "Promotion", $"{_navigationService.CurrentUser.FirstName + " " + _navigationService.CurrentUser.LastName} promoted {SelectedUser.FirstName + " " + SelectedUser.LastName} to manager");
                    await _userService.UpdateUserRole(SelectedUser.UserId, "Manager");
                    LoadUsers();

                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred while trying to promote a user in the user editor");
                _dialogService.ShowError("Error: An error occurred while trying to promote the user, please try again", "User Promotion Error");
            }
        }

        public async Task DemoteUser()
        {
            try
            {
                if (_selectedUser != null)
                {
                    await _actionLogService.CreateActionLog(_navigationService.CurrentUser, "Demotion", $"{_navigationService.CurrentUser.FirstName + " " + _navigationService.CurrentUser.LastName} demoted {SelectedUser.FirstName + " " + SelectedUser.LastName} from manager to associate");
                    await _userService.UpdateUserRole(SelectedUser.UserId, "Associate");
                    LoadUsers();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unexpected error occurred while attempting to demote a user in the user editor");
                _dialogService.ShowError("Error: An error occurred while trying to demote the user, please try again", "User Demotion Error");
            }
        }

        public async Task SaveChanges()
        {
            try
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
            catch (Exception ex)
            {
                Log.Error(ex, "An unexpected error occurred while attempting to save changes to the users in the user editor");
                _dialogService.ShowError("Error: An error occurred while trying to save changes to the users, please try again", "Save Changes Error");
            }
        }

        public async Task DeleteUser()
        {
            try
            {
                if (_selectedUser != null)
                {
                    await _userService.DeleteUser(SelectedUser.UserId);
                    Users.Remove(SelectedUser);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred while attempting to delete a user inside the user editor");
                _dialogService.ShowError("Error: An error occurred while trying to delete the user, please try again", "User Deletion Error");
            }
        }
    }
}
