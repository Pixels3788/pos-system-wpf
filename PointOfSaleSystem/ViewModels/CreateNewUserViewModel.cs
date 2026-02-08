using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Windows.Input;
using PointOfSaleSystem.Helpers;
using PointOfSaleSystem.Models;
using PointOfSaleSystem.Services;
using PointOfSaleSystem.Services.Interfaces;


namespace PointOfSaleSystem.ViewModels
{
    public class CreateNewUserViewModel : BaseViewModel
    {

        private readonly IUserService _userService;
        private readonly INavigationService _navigationService;

        private string? _firstName;
        
        public string? FirstName
        {
            get => _firstName;
            set
            {
                SetProperty(ref _firstName, value);
                ((RelayCommand)CreateUserCommand).RaiseCanExecuteChanged();
            }
        }

        private string? _lastName;

        public string? LastName
        {
            get => _lastName;
            set
            {
                SetProperty(ref _lastName, value);
                ((RelayCommand)CreateUserCommand).RaiseCanExecuteChanged();
            }
        }

        private string? _userEmail;

        public string? UserEmail
        {
            get => _userEmail;
            set
            {
                SetProperty(ref _userEmail, value);
                ((RelayCommand)CreateUserCommand).RaiseCanExecuteChanged();
            }
        }

        private string? _userPin;

        public string? UserPin
        {
            get => _userPin;
            set
            {
                SetProperty(ref _userPin, value);
                ((RelayCommand)CreateUserCommand).RaiseCanExecuteChanged();
            }
        }

        private string? _creationMessage;

        public string? CreationMessage
        {
            get => _creationMessage;
            set
            {
                SetProperty(ref _creationMessage, value);
                ((RelayCommand)CreateUserCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand CreateUserCommand { get;  }

        public CreateNewUserViewModel(INavigationService navigationService, IUserService userService)
        {
            _userService = userService;
            _navigationService = navigationService;
            CreateUserCommand = new RelayCommand(CreateUser, () => !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName)
                                                && !string.IsNullOrWhiteSpace(UserEmail) && !string.IsNullOrWhiteSpace(UserPin));
        }

        public void CreateUser()
        {
            if (_firstName == null) return;
            if (_lastName == null) return;
            if (_userEmail == null) return;
            if (_userPin == null) return;
            
            if (!int.TryParse(_userPin, out int newPin))
            {
                return;
            }

            User? newUser = _userService.CreateUser(FirstName, LastName, UserEmail, newPin);

            if (newUser == null)
            {
                CreationMessage = "User creation failed, please try again";
                return;
            }
            else
            {
                CreationMessage = "User Creation Succeeded! User has been registered";
            }
        }
 

    }
}
