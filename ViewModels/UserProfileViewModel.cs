
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Firebase.Auth;


using StudyFlow.Model;


using StudyFlow.Service.DBService;

namespace StudyFlow.ViewModels
{
    // אומר ל-Shell שכשמגיע פרמטר "UserId" בכתובת — שים אותו בתכונה UserId
    [QueryProperty(nameof(UserId), "UserId")]

    // partial — הטולקיט יוסיף קוד אוטומטי למחלקה
    public partial class UserProfileViewModel : ObservableObject
    {
        // שירות בסיס הנתונים — נגיש רק בתוך המחלקה ולא משתנה
        private readonly IAppUserRepository _dbService;

        // המשתמש הנוכחי — מעדכן את המסך אוטומטית בכל שינוי
        [ObservableProperty]
        private AppUser? _user;

        // מזהה המשתמש — מחרוזת ריקה כברירת מחדל
        private string _userId = string.Empty;

        // כשהערך משתנה — טוענת את המשתמש אוטומטית
        public string UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                // _ = אומר "אל תחכה לתוצאה" — מפעיל את הטעינה ברקע
                _ = LoadUserAsync(value);
            }
        }

        // מחזיר את השם המלא של המשתמש
        public string FullName => $"{User?.FirstName} {User?.LastName}";

        // מחזיר את התפקיד כטקסט — "Admin" או "Student"
        public string RoleLabel => User?.IsAdmin == true ? "Admin" : "Student";

        // מחזיר צבע לפי תפקיד — זהב למנהל, כחול כהה לתלמיד
        public Color RoleColor => User?.IsAdmin == true
            ? Color.FromArgb("#C9A84C")
            : Color.FromArgb("#1B2A4A");

        // קונסטרוקטור — מקבל את שירות בסיס הנתונים בהזרקת תלויות
        public UserProfileViewModel(IAppUserRepository dbService)
        {
            _dbService = dbService;
        }

        // טוענת את המשתמש לפי המזהה שלו
        private Task LoadUserAsync(string id)
        {
            // שולפת את כל המשתמשים ומחפשת לפי המזהה
            var users = _dbService.GetAllAsync();
            User = users.FirstOrDefault(u => u.Id == id);

            // מעדכנת ידנית תכונות מחושבות כי הטולקיט לא מטפל בהן אוטומטית
            OnPropertyChanged(nameof(FullName));
            OnPropertyChanged(nameof(RoleLabel));
            OnPropertyChanged(nameof(RoleColor));

            // מחזירה Task שסיים — כי אין פעולות אסינכרוניות
            return Task.CompletedTask;
        }

        // פקודה להפיכת משתמש למנהל — נקראת מכפתור על המסך
        [RelayCommand]
        private async Task MakeAdmin()
        {
            // אם אין משתמש נוכחי — יוצאת
            if (User is null) return;

            // מציגה דיאלוג אישור עם שם המשתמש
            bool confirm = await Application.Current!.MainPage!.DisplayAlert(
                "Make Admin",
                $"Make {User.FirstName} {User.LastName} an admin?",
                "Yes", "Cancel");

            // אם לחץ "Cancel" — יוצאת בלי לעשות כלום
            if (!confirm) return;

            // מעדכנת את המשתמש ב-Firebase
            await _dbService.SetToAdmin(User.Id);

            // מעדכנת את המשתמש בזיכרון
            User.IsAdmin = true;

            // מעדכנת את תצוגת התפקיד והצבע על המסך
            OnPropertyChanged(nameof(RoleLabel));
            OnPropertyChanged(nameof(RoleColor));

            // מציגה הודעת הצלחה
            await Application.Current!.MainPage!.DisplayAlert(
                "Success", $"{User.FirstName} is now an Admin!", "OK");
        }

        // פקודה למחיקת משתמש — נקראת מכפתור על המסך
        [RelayCommand]
        private async Task DeleteUser()
        {
            // אם אין משתמש נוכחי — יוצאת
            if (User is null) return;

            // מציגה דיאלוג אישור עם שם המשתמש
            bool confirm = await Application.Current!.MainPage!.DisplayAlert(
                "Delete User",
                $"Are you sure you want to delete {User.FirstName} {User.LastName}?",
                "Yes, Delete", "Cancel");

            // אם לחץ "Cancel" — יוצאת בלי לעשות כלום
            if (!confirm) return;

            // מוחקת את המשתמש מ-Firebase
            await _dbService.DeleteAsync(User);

            // חוזרת למסך הקודם
            await Shell.Current.GoToAsync("..");
        }

        // פקודה לחזרה למסך הקודם
        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}