using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Windows.Input;
using PointOfSaleSystem.Helpers;
using PointOfSaleSystem.Models;
using PointOfSaleSystem.Services;
using PointOfSaleSystem.Services.Interfaces;
using Serilog;

// View model for the user creation screen
namespace PointOfSaleSystem.ViewModels
{
    public class CreateNewUserViewModel : BaseViewModel
    {

        private readonly IUserService _userService;
        private readonly INavigationService _navigationService;
        private readonly IActionLogService _actionLogService;
        private readonly IDialogService _dialogService;

        private string? _firstName;
        
        public string? FirstName
        {
            get => _firstName;
            set
            {
                SetProperty(ref _firstName, value);
                ((AsyncRelayCommand)CreateUserCommand).RaiseCanExecuteChanged();
            }
        }

        private string? _lastName;

        public string? LastName
        {
            get => _lastName;
            set
            {
                SetProperty(ref _lastName, value);
                ((AsyncRelayCommand)CreateUserCommand).RaiseCanExecuteChanged();
            }
        }

        private string? _userEmail;

        public string? UserEmail
        {
            get => _userEmail;
            set
            {
                SetProperty(ref _userEmail, value);
                ((AsyncRelayCommand)CreateUserCommand).RaiseCanExecuteChanged();
            }
        }

        private string? _userPin;

        public string? UserPin
        {
            get => _userPin;
            set
            {
                SetProperty(ref _userPin, value);
                ((AsyncRelayCommand)CreateUserCommand).RaiseCanExecuteChanged();
            }
        }

        private string? _creationMessage;

        public string? CreationMessage
        {
            get => _creationMessage;
            set
            {
                SetProperty(ref _creationMessage, value);
                ((AsyncRelayCommand)CreateUserCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand CreateUserCommand { get;  }

        public ICommand NavigateToLoginCommand { get; }

        public CreateNewUserViewModel(INavigationService navigationService, IUserService userService, IActionLogService actionLogService, IDialogService dialogService)
        {
            _userService = userService;
            _navigationService = navigationService;
            _actionLogService = actionLogService;
            _dialogService = dialogService;
            CreateUserCommand = new AsyncRelayCommand(CreateUser, () => !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName)
                                                && !string.IsNullOrWhiteSpace(UserEmail) && !string.IsNullOrWhiteSpace(UserPin));
            NavigateToLoginCommand = new RelayCommand(BackToLogin);
            _actionLogService = actionLogService;
            _dialogService = dialogService;
        }

        public async Task CreateUser()
        {
            try
            {
                if (_firstName == null) return;
                if (_lastName == null) return;
                if (_userEmail == null) return;
                if (_userPin == null) return;

                if (!int.TryParse(_userPin, out int newPin))
                {
                    return;
                }

                User? newUser = await _userService.CreateUser(FirstName, LastName, UserEmail, newPin);

                if (newUser == null)
                {
                    CreationMessage = "User creation failed, please try again";
                    return;
                }
                else
                {
                    await _actionLogService.CreateActionLog(newUser, "Account Creation", $"A new account was created for {newUser.FirstName} {newUser.LastName}");
                    CreationMessage = "User Creation Succeeded! User has been registered";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred while attempting to create a new user");
                _dialogService.ShowError("Error, could not create a new user, please try again", "User Creation Error");
            }
        }

        public void BackToLogin()
        {
            try
            {
                _navigationService.Navigate<LoginScreenViewModel>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred while attempting to navigate to the login screen from the user creation screen");
                _dialogService.ShowError("Error, could not navigate to the login screen, please try again", "Navigation Error");
            }
        }
 

    }
}
