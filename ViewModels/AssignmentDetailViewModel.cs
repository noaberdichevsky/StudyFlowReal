// מייבא את הטולקיט לתמיכה ב-ObservableObject ו-RelayCommand
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

// מייבא את המודלים של הפרויקט
using StudyFlow.Model;

// מייבא את הממשקים של השירותים
using StudyFlow.Service.ClassroomService;
using StudyFlow.Service.DBService;

namespace StudyFlow.ViewModels
{
    // אומר ל-Shell שכשמגיע פרמטר "AssignmentId" בכתובת — שים אותו בתכונה AssignmentId
    [QueryProperty(nameof(AssignmentId), "AssignmentId")]
    // partial — הטולקיט יוסיף קוד אוטומטי למחלקה
    public partial class AssignmentDetailViewModel : ObservableObject
    {
        // שירות שמספק את רשימת המטלות
        private readonly IClassRooomService _classroomService;

        // שירות ששומר וטוען התקדמות תת-משימות
        private readonly IProgressRepository _progressRepository;

        // מזהה המשתמש המחובר
        private readonly string _userId;

        // מזהה המטלה הנוכחית — מחרוזת ריקה כברירת מחדל
        private string _assignmentId = string.Empty;

        // כשהערך משתנה — טוענת את המטלה אוטומטית
        public string AssignmentId
        {
            get => _assignmentId;
            set
            {
                _assignmentId = value;
                // _ = אומר "אל תחכה לתוצאה" — מפעיל את הטעינה ברקע
                _ = LoadAssignmentAsync(value);
            }
        }

        // המטלה הנוכחית — מעדכנת את המסך אוטומטית בכל שינוי
        [ObservableProperty]
        private CourseAssignment? _currentAssignment;

        // אחוז השלמת תת-המשימות — בין 0 ל-1
        [ObservableProperty]
        private double _completionProgress;

        // רשימת תת-המשימות של המטלה
        [ObservableProperty]
        private List<SubTask> _subtaskList = new();

        // כותרת תת-משימה חדשה שהמשתמש מקליד
        [ObservableProperty]
        private string _newSubtaskTitle = string.Empty;

        // ציון שהמשתמש מקליד — רק למטלות ידניות
        [ObservableProperty]
        private string _localScore = string.Empty;

        // מחזיר את הציון כטקסט — אם אין ציון מציג "Not graded yet"
        public string ScoreDisplay => CurrentAssignment?.Score > 0
            ? CurrentAssignment.Score.ToString("F0")
            : "Not graded yet";

        // בודק אם המטלה היא ידנית — קובע אם להציג שדה ציון
        public bool IsLocalAssignment => CurrentAssignment?.IsLocal ?? false;

        // קונסטרוקטור — מקבל את השירותים בהזרקת תלויות
        public AssignmentDetailViewModel(
            IClassRooomService classroomService,
            IProgressRepository progressRepository)
        {
            _classroomService = classroomService;
            _progressRepository = progressRepository;
            // שולפת את מזהה המשתמש המחובר
            _userId = (App.Current as App)?.CurrentUser?.Id ?? string.Empty;
        }

        // טוענת את המטלה לפי המזהה שלה
        private async Task LoadAssignmentAsync(string id)
        {
            // מחפשת את המטלה ברשימה לפי המזהה
            var found = _classroomService
                .GetAssignments()
                .FirstOrDefault(a => a.Id == id);

            // אם לא נמצאה — יוצאת מהפונקציה
            if (found == null) return;

            // מגדירה את המטלה הנוכחית ומעדכנת את המסך
            CurrentAssignment = found;

            // מעדכנת ידנית תכונות מחושבות כי הטולקיט לא מטפל בהן אוטומטית
            OnPropertyChanged(nameof(ScoreDisplay));
            OnPropertyChanged(nameof(IsLocalAssignment));

            // אם מטלה ידנית ויש ציון — טוענת אותו לשדה הקלט
            if (CurrentAssignment?.IsLocal == true)
                LocalScore = CurrentAssignment.Score > 0 ? CurrentAssignment.Score.ToString("F0") : string.Empty;

            // טוענת את ההתקדמות השמורה מ-Firebase
            var savedProgress = await _progressRepository
                .LoadProgressAsync(_userId, id);

            // אם יש התקדמות שמורה — מחילה אותה על תת-המשימות
            if (savedProgress != null && savedProgress.Count > 0)
            {
                // עוברת על תת-המשימות המקוריות ומעדכנת את מצב ההשלמה
                foreach (var subtask in found.Subtasks)
                {
                    // מחפשת את ההתקדמות השמורה לתת-משימה זו
                    var saved = savedProgress.FirstOrDefault(s => s.Id == subtask.Id);

                    // אם נמצאה — מעדכנת את מצב ההשלמה
                    if (saved != null)
                        subtask.IsCompleted = saved.IsCompleted;
                }

                // מוצאת תת-משימות שהמשתמש הוסיף ידנית — לא קיימות במטלה המקורית
                var customSubtasks = savedProgress
                    .Where(s => !found.Subtasks.Any(f => f.Id == s.Id))
                    .ToList();

                // מוסיפה את תת-המשימות הידניות לרשימה
                found.Subtasks.AddRange(customSubtasks);
            }

            // מאזינה לשינויים בכל תת-משימה — כשמסמנים/מבטלים סימון
            foreach (var subtask in found.Subtasks)
            {
                subtask.PropertyChanged += async (s, e) =>
                {
                    // אם השינוי הוא ב-IsCompleted — מעדכנת התקדמות ושומרת
                    if (e.PropertyName == nameof(SubTask.IsCompleted))
                    {
                        UpdateProgress();
                        await SaveProgressAsync();
                    }
                };
            }

            // מגדירה את רשימת תת-המשימות ומחשבת התקדמות
            SubtaskList = found.Subtasks;
            UpdateProgress();
        }

        // שומרת את ההתקדמות ל-Firebase
        private async Task SaveProgressAsync()
        {
            await _progressRepository.SaveProgressAsync(
                _userId,
                _assignmentId,
                SubtaskList);
        }

        // פקודה לשמירת ציון — נקראת מכפתור על המסך
        [RelayCommand]
        private async Task SaveScore()
        {
            // אם אין מטלה נוכחית — יוצאת
            if (CurrentAssignment is null) return;

            // מנסה להמיר את הציון שהוקלד למספר
            if (double.TryParse(LocalScore, out var score))
            {
                // שומרת את הציון במטלה ומעדכנת ב-Firebase
                CurrentAssignment.Score = score;
                await LocalAssignmentService.UpdateAssignment(CurrentAssignment);

                // מעדכנת את תצוגת הציון על המסך
                OnPropertyChanged(nameof(ScoreDisplay));

                // מציגה הודעת הצלחה
                await Application.Current!.MainPage!.DisplayAlert("Saved", "Score saved!", "OK");
            }
            else
            {
                // אם הקלט לא תקין — מציגה שגיאה
                await Application.Current!.MainPage!.DisplayAlert("Error", "Please enter a valid number!", "OK");
            }
        }

        // פקודה להוספת תת-משימה חדשה
        [RelayCommand]
        private async Task AddSubtask()
        {
            // אם השדה ריק — לא עושה כלום
            if (string.IsNullOrWhiteSpace(NewSubtaskTitle)) return;

            // יוצרת תת-משימה חדשה עם מזהה ייחודי
            var newSubtask = new SubTask
            {
                Id = Guid.NewGuid().ToString(),
                Title = NewSubtaskTitle,
                IsCompleted = false
            };

            // מאזינה לשינויים בתת-המשימה החדשה
            newSubtask.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(SubTask.IsCompleted))
                {
                    UpdateProgress();
                    await SaveProgressAsync();
                }
            };

            // יוצרת רשימה חדשה עם תת-המשימה החדשה בסוף
            var updated = new List<SubTask>(SubtaskList) { newSubtask };

            // מעדכנת את הרשימה ומנקה את שדה הקלט
            SubtaskList = updated;
            NewSubtaskTitle = string.Empty;

            // מעדכנת התקדמות ושומרת ל-Firebase
            UpdateProgress();
            await SaveProgressAsync();
        }

        // מחשבת את אחוז ההשלמה של תת-המשימות
        private void UpdateProgress()
        {
            // אם אין תת-משימות — אחוז ההשלמה הוא 0
            if (SubtaskList == null || SubtaskList.Count == 0)
            {
                CompletionProgress = 0;
                return;
            }

            // סופרת כמה תת-משימות הושלמו
            var completed = SubtaskList.Count(s => s.IsCompleted);

            // מחשבת אחוז — בין 0 ל-1
            CompletionProgress = (double)completed / SubtaskList.Count;
        }

        // פקודה לחזרה למסך הקודם
        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}