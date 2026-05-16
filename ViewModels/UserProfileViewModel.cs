using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Auth;
using StudyFlow.Model;
using StudyFlow.Service.DBService;

namespace StudyFlow.ViewModels
{
    [QueryProperty(nameof(UserId), "UserId")]
    public partial class UserProfileViewModel : ObservableObject
    {
        private readonly IAppUserRepository _dbService;

        [ObservableProperty]
        private AppUser? _user;

        private string _userId = string.Empty;
        public string UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                _ = LoadUserAsync(value);
            }
        }

        public string FullName => $"{User?.FirstName} {User?.LastName}";
        public string RoleLabel => User?.IsAdmin == true ? "Admin" : "Student";
        public Color RoleColor => User?.IsAdmin == true
            ? Color.FromArgb("#C9A84C")
            : Color.FromArgb("#1B2A4A");

        public UserProfileViewModel(IAppUserRepository dbService)
        {
            _dbService = dbService;
        }

        private Task LoadUserAsync(string id)
        {
            var users = _dbService.GetAllAsync();
            User = users.FirstOrDefault(u => u.Id == id);
            OnPropertyChanged(nameof(FullName));
            OnPropertyChanged(nameof(RoleLabel));
            OnPropertyChanged(nameof(RoleColor));
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task MakeAdmin()
        {
            if (User is null) return;

            bool confirm = await Application.Current!.MainPage!.DisplayAlert(
                "Make Admin",
                $"Make {User.FirstName} {User.LastName} an admin?",
                "Yes", "Cancel");
            if (!confirm) return;

            await _dbService.SetToAdmin(User.Id);
            User.IsAdmin = true;
            OnPropertyChanged(nameof(RoleLabel));
            OnPropertyChanged(nameof(RoleColor));
            await Application.Current!.MainPage!.DisplayAlert(
                "Success", $"{User.FirstName} is now an Admin!", "OK");
        }

        [RelayCommand]
        private async Task DeleteUser()
        {
            if (User is null) return;

            bool confirm = await Application.Current!.MainPage!.DisplayAlert(
                "Delete User",
                $"Are you sure you want to delete {User.FirstName} {User.LastName}?",
                "Yes, Delete", "Cancel");
            if (!confirm) return;

            await _dbService.DeleteAsync(User);
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}