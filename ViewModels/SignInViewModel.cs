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
            //טעינת מסך 
            IsBusy = true;
            try
            {
                //קורא לגוגל אאוט סרביס שפותח את קרום ומחכה לטוקן מגוגל 
                var token = await _googleAuthService.SignInAsync();

                System.Diagnostics.Debug.WriteLine($"Token: {token}");
                //בודק שיתקבל טוקן תקין
                if (!string.IsNullOrEmpty(token))
                {
                    //שומר טוקן באחסון מוצפן לשימוש עתידי
                    await SecureStorage.Default.SetAsync("google_access_token", token);
                    //אם סימן לזכור אותו שומר גם את זה כדי שבפתיחה הבאה יפתח מסך ראשי
                    if (IsRememberMeChecked)
                        await SecureStorage.Default.SetAsync("remember_google", "true");
                    //יוצר חיבור HTTP ומוסיף את הטוקן לכותרת הבקשה כדי שידע מי שואל 
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    //שולח בקשה לגוגל לקבלת פרטי המשתמש המחובר
                    var response = await client.GetStringAsync("https://www.googleapis.com/oauth2/v2/userinfo");
                    System.Diagnostics.Debug.WriteLine($"UserInfo: {response}");
                    //חילוץ ]רטי המשתמש
                    var userInfo = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(response);
                    var googleId = userInfo.GetProperty("id").GetString() ?? string.Empty;
                    var email = userInfo.GetProperty("email").GetString() ?? string.Empty;
                    var firstName = userInfo.GetProperty("given_name").GetString() ?? string.Empty;
                    var lastName = userInfo.GetProperty("family_name").GetString() ?? string.Empty;

                    // חפש משתמש קיים לפי Google ID
                    var allUsers = _dbService.GetAllAsync();
                    var existingUser = allUsers.FirstOrDefault(u => u.Id == googleId);
                    //אם משתמש קיים טוענים אותו 
                    if (existingUser != null)
                    {
                        (App.Current as App)!.CurrentUser = existingUser;
                    }
                    else
                    {
                        // אם לא יוצרים אובייקט חדש של משתמש 
                        var newUser = new StudyFlow.Model.AppUser
                        {
                            Id = googleId,
                            FirstName = firstName,
                            LastName = lastName,
                            UserEmail = email,
                            IsAdmin = false,
                            RegDate = DateTime.Now.ToString("dd/MM/yyyy")
                        };
                        //ומשתמשים בו כמשתמש נוכחי בפייר בייס 
                        var firebaseClient = new Firebase.Database.FirebaseClient(
                            "https://studyflowdb-default-rtdb.europe-west1.firebasedatabase.app/");
                        await firebaseClient
                            .Child("users")
                            .Child(newUser.Id)
                            .PutAsync(newUser);

                        (App.Current as App)!.CurrentUser = newUser;
                    }
                    //עוברים למסך הראשי
                    var mainPage = IPlatformApplication.Current!.Services.GetService<AppShell>();
                    Application.Current!.Windows[0].Page = mainPage;
                }
                //לא התקבל טוקן
                else
                {
                    ShowErrorMessage("Google Sign In failed!");
                }

                IsBusy = false;
            }//אם קרתה שגיאה 
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
            // בדוק Remember Me רגיל
            string? userId = await SecureStorage.Default.GetAsync("current_user_object");
            //בודק אם קיים היוזר איי די מהריממבר 
            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    //טוענת את פרטי המשתמש מהפיירבייס לפי היוזר איי די השמור ומגדירה אותו כמשתמש נוכחי 
                    IsBusy = true;
                    var user = await _dbService.GetUserByIdAsync(userId);
                    (App.Current as App)!.CurrentUser = user;
                    IsBusy = false;
                    //עברו למסך הראשי
                    var mainPage = IPlatformApplication.Current!.Services.GetService<AppShell>();
                    Application.Current!.Windows[0].Page = mainPage;
                    return;
                }
                catch (Exception ex)
                {
                    IsBusy = false;
                    ShowErrorMessage(ex.Message);
                }
            }

            // בודקת אם קיים טוקן גוגל שמור וסימון "זכור אותי" לגוגל.
            string? rememberGoogle = await SecureStorage.Default.GetAsync("remember_google");
            string? googleToken = await SecureStorage.Default.GetAsync("google_access_token");
            //אם שניהם קיימים — עוברת ישירות למסך הראשי בלי כניסה מחדש.
            if (rememberGoogle == "true" && !string.IsNullOrEmpty(googleToken))
            {
                var mainPage = IPlatformApplication.Current!.Services.GetService<AppShell>();
                Application.Current!.Windows[0].Page = mainPage;
                return;
            }

            // אם לא Remember Me — מחק את הטוקן
            SecureStorage.Default.Remove("google_access_token");
        }
    }
}