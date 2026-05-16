using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudyFlow.Model;
using StudyFlow.Service.ClassroomService;
using StudyFlow.Service.DBService;

namespace StudyFlow.ViewModels
{
    [QueryProperty(nameof(AssignmentId), "AssignmentId")]
    public partial class AssignmentDetailViewModel : ObservableObject
    {
        // The classroom service
        private readonly IClassRooomService _classroomService;

        // The progress repository - saves and loads subtask progress
        private readonly IProgressRepository _progressRepository;

        // The current logged in user
        private readonly string _userId;

        // The assignment ID passed from main page
        private string _assignmentId = string.Empty;
        public string AssignmentId
        {
            get => _assignmentId;
            set
            {
                _assignmentId = value;
                // Load assignment when ID is set
                _ = LoadAssignmentAsync(value);
            }
        }

        // The assignment being displayed
        [ObservableProperty]
        private CourseAssignment? _currentAssignment;

        // Progress percentage
        [ObservableProperty]
        private double _completionProgress;

        // Subtasks list
        [ObservableProperty]
        private List<SubTask> _subtaskList = new();

        // The new custom subtask text the user types
        [ObservableProperty]
        private string _newSubtaskTitle = string.Empty;

        // Shows score or "Not graded yet"
        public string ScoreDisplay => CurrentAssignment?.Score > 0
            ? CurrentAssignment.Score.ToString("F0")
            : "Not graded yet";

        // Constructor
        public AssignmentDetailViewModel(
            IClassRooomService classroomService,
            IProgressRepository progressRepository)
        {
            _classroomService = classroomService;
            _progressRepository = progressRepository;

            // Get current user ID from App
            _userId = (App.Current as App)?.CurrentUser?.Id ?? string.Empty;
        }

        // Loads the assignment and saved progress from Firebase
        private async Task LoadAssignmentAsync(string id)
        {
            // Find the assignment from classroom service
            var found = _classroomService
                .GetAssignments()
                .FirstOrDefault(a => a.Id == id);

            if (found == null) return;

            CurrentAssignment = found;

            // Try to load saved progress from Firebase
            var savedProgress = await _progressRepository
                .LoadProgressAsync(_userId, id);

            if (savedProgress != null && savedProgress.Count > 0)
            {
                // Merge saved progress with generated subtasks
                foreach (var subtask in found.Subtasks)
                {
                    var saved = savedProgress.FirstOrDefault(s => s.Id == subtask.Id);
                    if (saved != null)
                        subtask.IsCompleted = saved.IsCompleted;
                }

                // Also add any user-added custom subtasks that aren't in generated list
                var customSubtasks = savedProgress
                    .Where(s => !found.Subtasks.Any(f => f.Id == s.Id))
                    .ToList();

                found.Subtasks.AddRange(customSubtasks);
            }

            // Subscribe to each subtask's PropertyChanged
            // so progress bar updates when checkbox is tapped
            foreach (var subtask in found.Subtasks)
            {
                subtask.PropertyChanged += async (s, e) =>
                {
                    if (e.PropertyName == nameof(SubTask.IsCompleted))
                    {
                        UpdateProgress();
                        // Save progress to Firebase whenever a checkbox changes
                        await SaveProgressAsync();
                    }
                };
            }

            SubtaskList = found.Subtasks;
            UpdateProgress();
        }

        // Saves current subtask progress to Firebase
        private async Task SaveProgressAsync()
        {
            await _progressRepository.SaveProgressAsync(
                _userId,
                _assignmentId,
                SubtaskList);
        }

        // Called when user taps Add Subtask button
        [RelayCommand]
        private async Task AddSubtask()
        {
            // Don't add empty subtasks
            if (string.IsNullOrWhiteSpace(NewSubtaskTitle)) return;

            // Create new subtask
            var newSubtask = new SubTask
            {
                Id = Guid.NewGuid().ToString(),
                Title = NewSubtaskTitle,
                IsCompleted = false
            };

            // Subscribe to property changed
            newSubtask.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(SubTask.IsCompleted))
                {
                    UpdateProgress();
                    await SaveProgressAsync();
                }
            };

            // Add to the list
            var updated = new List<SubTask>(SubtaskList) { newSubtask };
            SubtaskList = updated;

            // Clear the input field
            NewSubtaskTitle = string.Empty;

            // Recalculate progress and save
            UpdateProgress();
            await SaveProgressAsync();
        }

        // Recalculates progress
        private void UpdateProgress()
        {
            if (SubtaskList == null || SubtaskList.Count == 0)
            {
                CompletionProgress = 0;
                return;
            }

            var completed = SubtaskList.Count(s => s.IsCompleted);
            CompletionProgress = (double)completed / SubtaskList.Count;
        }

        // Goes back
        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}