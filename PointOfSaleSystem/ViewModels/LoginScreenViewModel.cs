using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using PointOfSaleSystem.Helpers;
using PointOfSaleSystem.Models;
using PointOfSaleSystem.Services;
using PointOfSaleSystem.Services.Interfaces;

namespace PointOfSaleSystem.ViewModels
{
    public class LoginScreenViewModel : BaseViewModel
    {

        private readonly INavigationService _navigationService;

        private readonly IUserService _userService;

        private readonly IActionLogService _actionLogService;

        private string? _pinInput;

        public string? PinInput
        {
            get => _pinInput;
            set  
            {
                if (SetProperty(ref _pinInput, value))
                {
                    LoginMessage = string.Empty;
                    ((RelayCommand)AttemptLoginCommand).RaiseCanExecuteChanged();
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

        public LoginScreenViewModel(INavigationService navigationService, IUserService userService, IActionLogService actionLogService)
        {
            _navigationService = navigationService;
            _userService = userService;
            _actionLogService = actionLogService;
            AttemptLoginCommand = new RelayCommand(AttemptLogin, () => !string.IsNullOrEmpty(PinInput) && PinInput.Length >= 4 && PinInput.Length <= 6);
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
        }


        public void AttemptLogin()
        {
            
            if (!int.TryParse(_pinInput, out int inputtedPin))
            {
                return;
            }
            User? loggedUser = _userService.GetUserByPin(inputtedPin);

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
                _actionLogService.CreateActionLog(loggedUser, "Logged in", $"{loggedUser.FirstName} Logged into the POS");
                _navigationService.Navigate<OrderTakingScreenViewModel>();
            }
        }

        public void OpenUserCreationScreen()
        {
            _navigationService.Navigate<CreateNewUserViewModel>();
        }
    }
}
