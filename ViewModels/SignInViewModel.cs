using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Database.Query;
using StudyFlow.Helper;
using StudyFlow.Model;
using StudyFlow.Service;
using StudyFlow.Service.ClassroomService;
using StudyFlow.Service.DBService;
using StudyFlow.Views;

namespace StudyFlow.ViewModels
{
    public partial class SignInViewModel : ObservableObject
    {
        private readonly Page _page;
        private readonly IAppUserRepository _dbService;
        private readonly IClassRooomService _classroomService;
        private readonly IGoogleAuthService _googleAuthService;

        private string _userEmail = string.Empty;
        public string UserEmail
        {
            get => _userEmail;
            set
            {
                if (_userEmail != value)
                {
                    _userEmail = value;
                    OnPropertyChanged();
                    (SignInCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        private string _userPassword = string.Empty;
        public string UserPassword
        {
            get => _userPassword;
            set
            {
                if (_userPassword != value)
                {
                    _userPassword = value;
                    OnPropertyChanged();
                    (SignInCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        [ObservableProperty] private string _passwordIconCode = FontHelper.OPEN_EYE_ICON;
        [ObservableProperty] private bool _entryAsPassword = true;
        [ObservableProperty] private bool _signInMessageVisible;
        [ObservableProperty] private bool _isRememberMeChecked;
        [ObservableProperty] private bool _isBusy;
        [ObservableProperty] private string _errorMessage = string.Empty;

        public INavigation? Navigation { get; set; }
        public Command SignInCommand { get; }

        public SignInViewModel(
            SignUpView signUpView,
            IAppUserRepository dbService,
            IClassRooomService classroomService,
            IGoogleAuthService googleAuthService)
        {
            _page = signUpView;
            _dbService = dbService;
            _classroomService = classroomService;
            _googleAuthService = googleAuthService;

            SignInCommand = new Command(SignIn, () =>
                !(string.IsNullOrEmpty(UserEmail) || string.IsNullOrEmpty(UserPassword)));
        }

        private async void SignIn()
        {
            IsBusy = true;
            try
            {
                var user = await _dbService.SignInAsync(UserEmail!, UserPassword!);
                IsBusy = false;

                if (IsRememberMeChecked)
                    await SecureStorage.Default.SetAsync("current_user_object", user.Id);

                (App.Current as App)!.CurrentUser = user;

                var mainPage = IPlatformApplication.Current!.Services.GetService<AppShell>();
                Application.Current!.Windows[0].Page = mainPage;

                if (user.IsAdmin)
                {
                    await Task.Delay(100);
                    await Shell.Current.GoToAsync("//AdminView");
                }
            }
            catch (Exception ex)
            {
                IsBusy = false;
                ShowErrorMessage(ex.Message);
            }
        }

        [RelayCommand]
        private async Task SignInWithGoogle()
        {
            IsBusy = true;
            try
            {
                var token = await _googleAuthService.SignInAsync();
                System.Diagnostics.Debug.WriteLine($"Token: {token}");

                if (!string.IsNullOrEmpty(token))
                {
                    await SecureStorage.Default.SetAsync("google_access_token", token);

                    // קבל פרטי משתמש מ-Google
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    var response = await client.GetStringAsync("https://www.googleapis.com/oauth2/v2/userinfo");
                    System.Diagnostics.Debug.WriteLine($"UserInfo: {response}");

                    var userInfo = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(response);
                    var email = userInfo.GetProperty("email").GetString() ?? string.Empty;
                    var firstName = userInfo.GetProperty("given_name").GetString() ?? string.Empty;
                    var lastName = userInfo.GetProperty("family_name").GetString() ?? string.Empty;

                    // חפש משתמש קיים
                    var allUsers = _dbService.GetAllAsync();
                    var existingUser = allUsers.FirstOrDefault(u => u.UserEmail == email);

                    if (existingUser != null)
                    {
                        (App.Current as App)!.CurrentUser = existingUser;
                    }
                    else
                    {
                        // צור משתמש חדש
                        var newUser = new StudyFlow.Model.AppUser
                        {
                            Id = Guid.NewGuid().ToString(),
                            FirstName = firstName,
                            LastName = lastName,
                            UserEmail = email,
                            IsAdmin = false,
                            RegDate = DateTime.Now.ToString("dd/MM/yyyy")
                        };
                        var firebaseClient = new Firebase.Database.FirebaseClient(
    "https://studyflowdb-default-rtdb.europe-west1.firebasedatabase.app/");
                        await firebaseClient
                            .Child("AppUsers")
                            .Child(newUser.Id)
                            .PutAsync(newUser);
                        (App.Current as App)!.CurrentUser = newUser;
                    }

                    var mainPage = IPlatformApplication.Current!.Services.GetService<AppShell>();
                    Application.Current!.Windows[0].Page = mainPage;
                }
                else
                {
                    ShowErrorMessage("Google Sign In failed!");
                }

                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                ShowErrorMessage($"Google Sign In failed: {ex.Message}");
            }
        }

        [RelayCommand]
        private void TogglePassword()
        {
            EntryAsPassword = !EntryAsPassword;
            PasswordIconCode = EntryAsPassword
                ? FontHelper.OPEN_EYE_ICON
                : FontHelper.CLOSED_EYE_ICON;
        }

        [RelayCommand]
        private async Task NavigateToSignUp()
        {
            await Navigation!.PushAsync(_page);
        }

        private void ShowErrorMessage(string message)
        {
            SignInMessageVisible = true;
            ErrorMessage = message;
        }

        public async void OnAppearing()
        {
            string? token = await SecureStorage.Default.GetAsync("current_user_object");
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    IsBusy = true;
                    var user = await _dbService.GetUserByIdAsync(token);
                    (App.Current as App)!.CurrentUser = user;
                    IsBusy = false;

                    var mainPage = IPlatformApplication.Current!.Services.GetService<AppShell>();
                    Application.Current!.Windows[0].Page = mainPage;
                }
                catch (Exception ex)
                {
                    IsBusy = false;
                    ShowErrorMessage(ex.Message);
                }
            }
        }
    }
}