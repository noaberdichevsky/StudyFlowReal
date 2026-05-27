using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudyFlow.Model;

// ייבוא חדש — נצטרך אותו לקריאה ל-Clear()
using StudyFlow.Service.ClassroomService;

namespace StudyFlow.ViewModels
{
    public partial class AppShellViewModel : ObservableObject
    {
        // שם המשתמש המוצג בתפריט — מעדכן את המסך אוטומטית
        [ObservableProperty]
        private string? _userName;

        // המשתמש המחובר כרגע
        private AppUser? _currentUser;

        // קונסטרוקטור — שולף את שם המשתמש המחובר
        public AppShellViewModel()
        {
            // שולפת את המשתמש הנוכחי מהאפליקציה
            _currentUser = (App.Current as App)?.CurrentUser;

            // יוצרת את השם המלא לתצוגה בתפריט
            _userName = $"{_currentUser?.FirstName} {_currentUser?.LastName}";
        }

        [RelayCommand]
        private async Task SignOut()
        {
            // שורה חדשה — מאפסת את המטלות הסטטיות לפני היציאה
            // בלי זה משתמש הבא יראה את המטלות של המשתמש הקודם
            LocalAssignmentService.Clear();

            // מוחקת את ה-UserId השמור של משתמש רגיל
            SecureStorage.Default.Remove("current_user_object");

            // מוחקת את הטוקן של גוגל
            SecureStorage.Default.Remove("google_access_token");

            // מוחקת את סימון "זכור אותי" של גוגל
            SecureStorage.Default.Remove("remember_google");

            // מאפסת את המשתמש הנוכחי בזיכרון האפליקציה
            (App.Current as App)!.CurrentUser = null;

            // שולפת את מסך הכניסה מה-DI Container
            var signInView = IPlatformApplication.Current!.Services.GetService<Views.SignInView>();

            // מחליפה את המסך הנוכחי במסך הכניסה
            Application.Current!.Windows[0].Page = new NavigationPage(signInView);
        }

        [RelayCommand]
        private async Task NavigateToAccount()
        {
            // עוברת למסך החשבון האישי
            await Shell.Current.GoToAsync("AccountView");
        }
    }
}