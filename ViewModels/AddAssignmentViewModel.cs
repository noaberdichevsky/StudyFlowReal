using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudyFlow.Model;
using StudyFlow.Service;
using StudyFlow.Service.ClassroomService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StudyFlow.ViewModels
{
    public partial class AddAssignmentViewModel : ObservableObject
    {
        private readonly SubtaskGeneratorService _subtaskGenerator;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _subject = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private DateTime _deadline = DateTime.Today.AddDays(7);

        public AddAssignmentViewModel(SubtaskGeneratorService subtaskGenerator)
        {
            //שומר את השירות במשתנה פרטי לשימוש בפונקציה Save.
            _subtaskGenerator = subtaskGenerator;
        }

        [RelayCommand]
        //פונקציה פרטית אסינכרונית שנקראת כשהמשתמש לוחץ "שמור" במסך הוספת מטלה.
        private async Task Save()
        {
            //בודקת שהמשתמש מילא את שדות החובה — כותרת ומקצוע. אם לא — מציגה הודעת שגיאה ומפסיקה.
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Subject))
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Error", "Please fill in Title and Subject!", "OK");
                return;
            }
            //יוצרת אובייקט מטלה חדש:
            //Guid.NewGuid() — יוצר מזהה ייחודי אקראי
            //Title, Subject, Description — ערכים שהמשתמש הקליד
            //Deadline.ToString("dd/MM/yyyy") — ממירה את התאריך למחרוזת
            //Score = 0 — ציון התחלתי אפס
            //Status = 0 — סטטוס חדש
            var assignment = new CourseAssignment
            {
                Id = Guid.NewGuid().ToString(),
                Title = Title,
                Subject = Subject,
                Description = Description,
                Deadline = Deadline.ToString("dd/MM/yyyy"),
                Score = 0,
                Status = 0
            };
            //יוצרת תת-משימות אוטומטיות לפי המקצוע שהמשתמש בחר.
            assignment.Subtasks = _subtaskGenerator.GenerateSubtasks(assignment, assignment.Id);
            //מוסיפה את המטלה לזיכרון ול-Firebase.
            await LocalAssignmentService.Add(assignment);
            //חוזרת למסך הקודם — .. אומר "עלה מסך אחד למעלה"
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task GoBack()
        {
            //חוזרת למסך הקודם — .. אומר "עלה מסך אחד למעלה" בניווט של Shell.
            await Shell.Current.GoToAsync("..");
        }
    }
}
