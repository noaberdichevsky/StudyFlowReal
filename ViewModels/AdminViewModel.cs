using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyFlow.ViewModels
{
    public partial class AdminViewModel : ObservableObject
    {
        // Controls whether debug mode is on or off
        [ObservableProperty]
        private bool _isDebugMode;

        // Constructor
        public AdminViewModel()
        {
        }

        // Navigates to the Users List page
        [RelayCommand]
        private async Task NavigateToUsersListView()
        {
            await Shell.Current.GoToAsync("UsersListView");
        }
    }
}
