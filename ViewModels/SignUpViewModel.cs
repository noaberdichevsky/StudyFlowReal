using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudyFlow.Helper;
using StudyFlow.Model;
using StudyFlow.Service.DBService;

namespace StudyFlow.ViewModels
{
    public partial class SignUpViewModel : ObservableObject
    {
        // The user repository - handles all user database operations
        private readonly IAppUserRepository _dbService;

        // First name field - manually implemented to trigger CanExecute
        private string _fName = string.Empty;
        public string FName
        {
            get => _fName;
            set
            {
                if (_fName != value)
                {
                    _fName = value;
                    OnPropertyChanged();
                    (SignUpCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        // Last name field
        private string _lName = string.Empty;
        public string LName
        {
            get => _lName;
            set
            {
                if (_lName != value)
                {
                    _lName = value;
                    OnPropertyChanged();
                    (SignUpCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        // Email field
        private string _uEmail = string.Empty;
        public string UEmail
        {
            get => _uEmail;
            set
            {
                if (_uEmail != value)
                {
                    _uEmail = value;
                    OnPropertyChanged();
                    (SignUpCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        // Password field
        private string _uPassword = string.Empty;
        public string UPassword
        {
            get => _uPassword;
            set
            {
                if (_uPassword != value)
                {
                    _uPassword = value;
                    OnPropertyChanged();
                    (SignUpCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        // Mobile field
        private string _uMobile = string.Empty;
        public string UMobile
        {
            get => _uMobile;
            set
            {
                if (_uMobile != value)
                {
                    _uMobile = value;
                    OnPropertyChanged();
                    (SignUpCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        // Controls whether the loading spinner is visible
        [ObservableProperty]
        private bool _isBusy;

        // Icon code for show/hide password button
        [ObservableProperty]
        private string _passwordIconCode = FontHelper.OPEN_EYE_ICON;

        // True = password hidden, False = password visible
        [ObservableProperty]
        private bool _entryAsPassword = true;

        // Controls whether error message is visible
        [ObservableProperty]
        private bool _signUpMessageVisible;

        // The error message text
        [ObservableProperty]
        private string _errorMessage = string.Empty;

        // Navigation object - set from the View
        public INavigation? Navigation { get; set; }

        // The SignUp button command
        public Command SignUpCommand { get; }

        // Constructor - receives IAppUserRepository via Dependency Injection
        public SignUpViewModel(IAppUserRepository dbService)
        {
            _dbService = dbService;

            // SignUp button only enabled when all fields pass validation
            SignUpCommand = new Command(SignUp, Validate);
        }

        // Called when Sign Up button is tapped
        private async void SignUp()
        {
            // Show loading spinner
            IsBusy = true;

            // Create a new AppUser object from the form fields
            var newUser = new AppUser()
            {
                FirstName = FName,
                LastName = LName,
                UserEmail = UEmail,
                UserPassword = UPassword,
                UserMobile = UMobile,
                // Save today's date as registration date
                RegDate = DateTime.Now.ToShortDateString(),
                UBDate = DateTime.Now.ToShortDateString()
            };

            try
            {
                // Save the new user to Firebase and get back their userId
                newUser.Id = await _dbService.CreateAsync(newUser);

                IsBusy = false;

                // Save the logged in user to the App class
                (App.Current as App)!.CurrentUser = newUser;

                // Navigate to the main app shell
                var mainPage = IPlatformApplication.Current!.Services.GetService<AppShell>();
                Application.Current!.Windows[0].Page = mainPage;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                ShowErrorMessage(ex.Message);
            }
        }

        // Toggles password between hidden and visible
        [RelayCommand]
        private void TogglePassword()
        {
            EntryAsPassword = !EntryAsPassword;
            PasswordIconCode = EntryAsPassword
                ? FontHelper.OPEN_EYE_ICON
                : FontHelper.CLOSED_EYE_ICON;
        }

        // Navigates back to SignIn page
        [RelayCommand]
        private async Task NavigateToSignIn()
        {
            await Navigation!.PopAsync();
        }

        // Validates all fields before enabling the SignUp button
        private bool Validate()
        {
            // First name must not be empty
            var fnameOK = !string.IsNullOrEmpty(FName);
            // Last name must not be empty
            var lnameOK = !string.IsNullOrEmpty(LName);
            // Email must not be empty
            var emailOK = !string.IsNullOrEmpty(UEmail);
            // Password must be at least 6 characters
            var passOK = !string.IsNullOrEmpty(UPassword) && UPassword.Length > 5;
            // Mobile must be exactly 10 digits
            var mobileOK = !string.IsNullOrEmpty(UMobile) && UMobile.Length == 10;

            return fnameOK && lnameOK && emailOK && passOK && mobileOK;
        }

        // Shows the error message label with the given message
        private void ShowErrorMessage(string message)
        {
            SignUpMessageVisible = true;
            ErrorMessage = message;
        }
    }
}