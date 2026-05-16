using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudyFlow.Model;
using StudyFlow.Service.DBService;
using System.Collections.ObjectModel;

namespace StudyFlow.ViewModels
{
    public partial class UsersListViewModel : ObservableObject
    {
        // The user repository - handles all user database operations
        private readonly IAppUserRepository _dbService;

        // The list of all users shown in the collection view
        [ObservableProperty]
        private ObservableCollection<AppUser> _allUsers = new();

        // The currently selected user in the collection view
        [ObservableProperty]
        private AppUser? _selectedUser;

        // Search text for filtering users
        [ObservableProperty]
        private string _searchText = string.Empty;

        // Controls whether the loading spinner is visible
        [ObservableProperty]
        private bool _isBusy;

        // Constructor - receives IAppUserRepository via Dependency Injection
        public UsersListViewModel(IAppUserRepository dbService)
        {
            _dbService = dbService;
        }

        // Called every time this page appears
        public void OnAppearing()
        {
            LoadUsers();
        }

        // Called every time this page disappears
        public void OnDisappearing()
        {
            // Clear selected user when leaving page
            SelectedUser = null;
        }

        // Loads all users from the database
        private void LoadUsers()
        {
            IsBusy = true;
            try
            {
                // Get all users from the database
                var users = _dbService.GetAllAsync();

                // Put them in the ObservableCollection so the UI updates
                AllUsers = new ObservableCollection<AppUser>(users);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadUsers failed: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Navigates to the Account page for the selected user
        [RelayCommand]
        private async Task NavigateToAccountPage()
        {
            if (SelectedUser == null) return;
            await Shell.Current.GoToAsync("AccountView");
        }
        // Deletes a user from Firebase Auth and Realtime Database
        [RelayCommand]
        private async Task DeleteUser(AppUser user)
        {
            // Ask admin to confirm before deleting
            bool confirm = await Application.Current!.MainPage!.DisplayAlert(
                "Delete User",
                $"Are you sure you want to delete {user.FirstName} {user.LastName}?",
                "Yes, Delete",
                "Cancel");

            if (!confirm) return;

            IsBusy = true;
            try
            {
                // Delete user from Firebase
                await _dbService.DeleteAsync(user);

                // Remove from the list so UI updates immediately
                AllUsers.Remove(user);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteUser failed: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert(
                    "Error",
                    "Could not delete user. Please try again.",
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}