using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using PointOfSaleSystem.Helpers;
using PointOfSaleSystem.Models;
using PointOfSaleSystem.Services;
using PointOfSaleSystem.Services.Interfaces;
using Serilog;

// View model for the login screen
namespace PointOfSaleSystem.ViewModels
{
    public class LoginScreenViewModel : BaseViewModel
    {

        private readonly INavigationService _navigationService;

        private readonly IUserService _userService;

        private readonly IActionLogService _actionLogService;

        private readonly IDialogService _dialogService;

        private string? _pinInput;

        public string? PinInput
        {
            get => _pinInput;
            set  
            {
                if (SetProperty(ref _pinInput, value))
                {
                    LoginMessage = string.Empty;
                    ((AsyncRelayCommand)AttemptLoginCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private string? _loginMessage;

        public string? LoginMessage
        {
            get => _loginMessage;
            set => SetProperty(ref _loginMessage, value);
        }

        public ICommand AttemptLoginCommand { get; }

        public ICommand EnterDigitCommand { get; }

        public ICommand DeleteDigitCommand { get; }

        public ICommand OpenCreationMenuCommand { get;  }

        public LoginScreenViewModel(INavigationService navigationService, IUserService userService, IActionLogService actionLogService, IDialogService dialogService)
        {
            _navigationService = navigationService;
            _userService = userService;
            _actionLogService = actionLogService;
            _dialogService = dialogService;
            AttemptLoginCommand = new AsyncRelayCommand(AttemptLogin, () => !string.IsNullOrEmpty(PinInput) && PinInput.Length >= 4 && PinInput.Length <= 6);
            EnterDigitCommand = new RelayCommand<string>(digit =>
            {
                if (PinInput == null) PinInput = string.Empty;
                if (PinInput.Length < 6)
                    PinInput += digit;
            });
            DeleteDigitCommand = new RelayCommand(() =>
            {
                if (!string.IsNullOrEmpty(PinInput))
                    PinInput = PinInput[..^1];
            });
            OpenCreationMenuCommand = new RelayCommand(OpenUserCreationScreen);
            _actionLogService = actionLogService;
            _dialogService = dialogService;
        }


        public async Task AttemptLogin()
        {
            try
            {
                if (!int.TryParse(_pinInput, out int inputtedPin))
                {
                    return;
                }
                User? loggedUser = await _userService.GetUserByPin(inputtedPin);

                if (loggedUser == null)
                {
                    PinInput = string.Empty;
                    LoginMessage = "Invalid pin, please try again";
                    return;
                }
                else
                {
                    LoginMessage = "Successful Login";
                    _navigationService.SetCurrentUser(loggedUser);
                    await _actionLogService.CreateActionLog(loggedUser, "Logged in", $"{loggedUser.FirstName} Logged into the POS");
                    _navigationService.Navigate<OrderTakingScreenViewModel>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred while the user attempted to log in");
                _dialogService.ShowError("Error: An error occurred while trying to log you in, please try again", "Login Error");
            }
        }

        public void OpenUserCreationScreen()
        {
            try
            {
                _navigationService.Navigate<CreateNewUserViewModel>();
            }
            catch (Exception ex)
            {
                Log.Error("Unexpected error occurred while attempting to navigate to the user creation screen inside of the login viewmodel");
                _dialogService.ShowError("Error: An error occurred while attempting to navigate to the user creation screen, please try again", "Navigation Error");
            }
        }
    }
}
