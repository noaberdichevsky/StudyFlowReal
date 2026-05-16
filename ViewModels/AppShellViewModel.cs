using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudyFlow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyFlow.ViewModels
{
    public partial class AppShellViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _userName;

        private AppUser? _currentUser;

        public AppShellViewModel()
        {
            _currentUser = (App.Current as App)?.CurrentUser;
            _userName = $"{_currentUser?.FirstName} {_currentUser?.LastName}";
        }

        // Signs the user out and navigates back to Sign In page
        [RelayCommand]
        private async Task SignOut()
        {
            // Remove saved userId from secure storage (clears RememberMe)
            SecureStorage.Default.Remove("current_user_object");

            // Clear the current user from the App class
            (App.Current as App)!.CurrentUser = null;

            // Navigate back to Sign In page
            var signInView = IPlatformApplication.Current!.Services.GetService<Views.SignInView>();
            Application.Current!.Windows[0].Page = new NavigationPage(signInView);
        }

        [RelayCommand]
        private async Task NavigateToAccount()
        {
            await Shell.Current.GoToAsync("AccountView");
        }
    }
}