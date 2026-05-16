using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudyFlow.Model;
using StudyFlow.Service.DBService;

namespace StudyFlow.ViewModels
{
    public partial class AccountViewModel : ObservableObject
    {
        // The user repository - handles all user database operations
        private readonly IAppUserRepository _dbService;

        // The current logged in user
        private AppUser? _currentUser;

        // First name field - bound to the entry in the view
        [ObservableProperty]
        private string _firstName = string.Empty;

        // Last name field
        [ObservableProperty]
        private string _lastName = string.Empty;

        // Email field - read only, user cannot change their email
        [ObservableProperty]
        private string _userEmail = string.Empty;

        // Mobile field
        [ObservableProperty]
        private string _userMobile = string.Empty;

        // Controls whether the loading spinner is visible
        [ObservableProperty]
        private bool _isBusy;

        // Controls whether the error message is visible
        [ObservableProperty]
        private bool _errorMessageIsVisible;

        // The error message text
        [ObservableProperty]
        private string _errorMessage = string.Empty;

        // Constructor - receives IAppUserRepository via Dependency Injection
        public AccountViewModel(IAppUserRepository dbService)
        {
            _dbService = dbService;

            // Get the current logged in user from the App class
            _currentUser = (App.Current as App)?.CurrentUser;

            // Fill the form fields with the current user's data
            FirstName = _currentUser?.FirstName ?? string.Empty;
            LastName = _currentUser?.LastName ?? string.Empty;
            UserEmail = _currentUser?.UserEmail ?? string.Empty;
            UserMobile = _currentUser?.UserMobile ?? string.Empty;
        }

        // Called when Update button is tapped
        [RelayCommand]
        private async Task Update()
        {
            IsBusy = true;
            try
            {
                // Update the current user object with the new values from the form
                _currentUser!.FirstName = FirstName;
                _currentUser!.LastName = LastName;
                _currentUser!.UserMobile = UserMobile;

                // Save the updated user to Firebase
                await _dbService.UpdateAsync(_currentUser!);

                // Update the App.CurrentUser with the new data
                (App.Current as App)!.CurrentUser = _currentUser;

                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                ErrorMessageIsVisible = true;
                ErrorMessage = ex.Message;
            }
        }

        // Called when Sign Out is tapped
        [RelayCommand]
        private async Task SignOut()
        {
            // Remove the saved userId from secure storage (clears RememberMe)
            SecureStorage.Default.Remove("current_user_object");

            // Clear the current user from the App class
            (App.Current as App)!.CurrentUser = null;

            // Navigate back to SignIn page
            var signInPage = IPlatformApplication.Current!.Services.GetService<Views.SignInView>();
            Application.Current!.Windows[0].Page = new NavigationPage(signInPage);
        }
        // Deletes the current user's own account
        [RelayCommand]
        private async Task DeleteAccount()
        {
            // Ask user to confirm before deleting
            bool confirm = await Application.Current!.MainPage!.DisplayAlert(
                "Delete Account",
                "Are you sure you want to permanently delete your account? This cannot be undone!",
                "Yes, Delete",
                "Cancel");

            if (!confirm) return;

            IsBusy = true;
            try
            {
                // Delete from Firebase Auth and Realtime Database
                await _dbService.DeleteAsync(_currentUser!);

                // Clear secure storage
                SecureStorage.Default.Remove("current_user_object");

                // Clear current user
                (App.Current as App)!.CurrentUser = null;

                // Navigate back to Sign In page
                var signInView = IPlatformApplication.Current!.Services.GetService<Views.SignInView>();
                Application.Current!.Windows[0].Page = new NavigationPage(signInView);
            }
            catch (Exception ex)
            {
                IsBusy = false;
                ErrorMessageIsVisible = true;
                ErrorMessage = "Could not delete account. Please try again.";
                System.Diagnostics.Debug.WriteLine($"DeleteAccount failed: {ex.Message}");
            }
        }
    }
}