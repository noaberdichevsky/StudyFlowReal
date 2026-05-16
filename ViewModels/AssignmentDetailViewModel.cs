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
        private readonly IClassRooomService _classroomService;
        private readonly IProgressRepository _progressRepository;
        private readonly string _userId;

        private string _assignmentId = string.Empty;
        public string AssignmentId
        {
            get => _assignmentId;
            set
            {
                _assignmentId = value;
                _ = LoadAssignmentAsync(value);
            }
        }

        [ObservableProperty]
        private CourseAssignment? _currentAssignment;

        [ObservableProperty]
        private double _completionProgress;

        [ObservableProperty]
        private List<SubTask> _subtaskList = new();

        [ObservableProperty]
        private string _newSubtaskTitle = string.Empty;

        public string ScoreDisplay => CurrentAssignment?.Score > 0
            ? CurrentAssignment.Score.ToString("F0")
            : "Not graded yet";

        public AssignmentDetailViewModel(
            IClassRooomService classroomService,
            IProgressRepository progressRepository)
        {
            _classroomService = classroomService;
            _progressRepository = progressRepository;
            _userId = (App.Current as App)?.CurrentUser?.Id ?? string.Empty;
        }

        private async Task LoadAssignmentAsync(string id)
        {
            var found = _classroomService
                .GetAssignments()
                .FirstOrDefault(a => a.Id == id);

            if (found == null) return;

            CurrentAssignment = found;
            OnPropertyChanged(nameof(ScoreDisplay));

            var savedProgress = await _progressRepository
                .LoadProgressAsync(_userId, id);

            if (savedProgress != null && savedProgress.Count > 0)
            {
                foreach (var subtask in found.Subtasks)
                {
                    var saved = savedProgress.FirstOrDefault(s => s.Id == subtask.Id);
                    if (saved != null)
                        subtask.IsCompleted = saved.IsCompleted;
                }

                var customSubtasks = savedProgress
                    .Where(s => !found.Subtasks.Any(f => f.Id == s.Id))
                    .ToList();

                found.Subtasks.AddRange(customSubtasks);
            }

            foreach (var subtask in found.Subtasks)
            {
                subtask.PropertyChanged += async (s, e) =>
                {
                    if (e.PropertyName == nameof(SubTask.IsCompleted))
                    {
                        UpdateProgress();
                        await SaveProgressAsync();
                    }
                };
            }

            SubtaskList = found.Subtasks;
            UpdateProgress();
        }

        private async Task SaveProgressAsync()
        {
            await _progressRepository.SaveProgressAsync(
                _userId,
                _assignmentId,
                SubtaskList);
        }

        [RelayCommand]
        private async Task AddSubtask()
        {
            if (string.IsNullOrWhiteSpace(NewSubtaskTitle)) return;

            var newSubtask = new SubTask
            {
                Id = Guid.NewGuid().ToString(),
                Title = NewSubtaskTitle,
                IsCompleted = false
            };

            newSubtask.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(SubTask.IsCompleted))
                {
                    UpdateProgress();
                    await SaveProgressAsync();
                }
            };

            var updated = new List<SubTask>(SubtaskList) { newSubtask };
            SubtaskList = updated;
            NewSubtaskTitle = string.Empty;
            UpdateProgress();
            await SaveProgressAsync();
        }

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

        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}