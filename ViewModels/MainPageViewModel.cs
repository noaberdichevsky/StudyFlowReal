using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudyFlow.Model;
using StudyFlow.Service;
using StudyFlow.Service.ClassroomService;
using StudyFlow.Service.DBService;
using System.Collections.ObjectModel;

namespace StudyFlow.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        // The classroom service - provides assignments
        private readonly IClassRooomService _classroomService;

        // The progress repository - loads saved subtask progress
        private readonly IProgressRepository _progressRepository;

        // The current user ID
        private readonly string _userId;

        // The list of assignments shown in the collection view
        [ObservableProperty]
        private ObservableCollection<CourseAssignment> _assignments = new();

        // The average score displayed at the top of the page
        [ObservableProperty]
        private double _averageScore;

        // Controls whether the loading spinner is visible
        [ObservableProperty]
        private bool _isBusy;

        // The currently selected assignment
        [ObservableProperty]
        private CourseAssignment? _selectedAssignment;

        // Constructor - receives services via Dependency Injection
        public MainPageViewModel(
            IClassRooomService classroomService,
            IProgressRepository progressRepository)
        {
            _classroomService = classroomService;
            _progressRepository = progressRepository;
            _userId = (App.Current as App)?.CurrentUser?.Id ?? string.Empty;
        }

        // Called every time the Main Page appears
        public async Task OnAppearing()
        {
            await LoadAssignments();
        }

        // Refreshes assignments and loads saved progress
        private async Task LoadAssignments()
        {
            IsBusy = true;
            try
            {
                // Refresh assignments from service
                await _classroomService.RefreshAssignments();

                var assignments = _classroomService.GetAssignments();

                // Load saved progress for each assignment
                foreach (var assignment in assignments)
                {
                    var savedProgress = await _progressRepository
                        .LoadProgressAsync(_userId, assignment.Id);

                    if (savedProgress != null && savedProgress.Count > 0)
                    {
                        // Merge saved progress with generated subtasks
                        foreach (var subtask in assignment.Subtasks)
                        {
                            var saved = savedProgress
                                .FirstOrDefault(s => s.Id == subtask.Id);
                            if (saved != null)
                                subtask.IsCompleted = saved.IsCompleted;
                        }

                        // Add user custom subtasks that aren't in generated list
                        var customSubtasks = savedProgress
                            .Where(s => !assignment.Subtasks.Any(f => f.Id == s.Id))
                            .ToList();

                        assignment.Subtasks.AddRange(customSubtasks);
                    }
                }

                // Put in ObservableCollection so UI updates
                Assignments = new ObservableCollection<CourseAssignment>(assignments);

                // Calculate average score
                CalculateAverageScore();
                NotificationService.ScheduleReminders(assignments.ToList());

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadAssignments failed: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Calculates average score from graded assignments
        private void CalculateAverageScore()
        {
            var graded = Assignments.Where(a => a.Score > 0).ToList();

            if (!graded.Any())
            {
                AverageScore = 0;
                return;
            }

            AverageScore = graded.Average(a => a.Score);
        }

        // Called when an assignment card is tapped
        [RelayCommand]
        private async Task NavigateToDetail()
        {
            if (SelectedAssignment is null) return;

            // Pass the assignment ID to the detail page
            await Shell.Current.GoToAsync(
                $"AssignmentDetailView?AssignmentId={SelectedAssignment.Id}");
        }
        [RelayCommand]
        private async Task AddAssignment()
        {
            await Shell.Current.GoToAsync("AddAssignmentView");
        }
        [RelayCommand]
        private async Task DeleteAssignment(CourseAssignment assignment)
        {
            //בודק אם המטלה היא לוקליצ אט לא
            var isLocal = LocalAssignmentService.GetAll().Any(a => a.Id == assignment.Id);
            // אם לא
            if (!isLocal)
            {
                //שולח הודעה שאי אפשר למחוק 
                await Application.Current!.MainPage!.DisplayAlert(
                    "Cannot Delete",
                    "You can only delete assignments you added manually.",
                    "OK");
                return;
            }
            // אם כן אז מוחק אותה 
            //מוחקת את המטלה מהזיכרון ומ-Firebase לצמיתות — פונה ל-LocalAssignmentService.
            await LocalAssignmentService.Remove(assignment.Id);
            //מוחקת את המטלה מה-ObservableCollection — המסך מתעדכן אוטומטית ומציג את הרשימה בלי המטלה שנמחקה.
            Assignments.Remove(assignment);
        }
    }
}