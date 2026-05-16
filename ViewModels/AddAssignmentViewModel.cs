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
            _subtaskGenerator = subtaskGenerator;
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Subject))
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Error", "Please fill in Title and Subject!", "OK");
                return;
            }

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

            assignment.Subtasks = _subtaskGenerator.GenerateSubtasks(assignment, assignment.Id);
            await LocalAssignmentService.Add(assignment);

            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
