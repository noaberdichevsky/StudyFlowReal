using StudyFlow.Model;

namespace StudyFlow.Service.ClassroomService
{
    public class MockClassroomService : IClassRooomService
    {
        private readonly SubtaskGeneratorService _subtaskGenerator;
        private List<CourseAssignment> _assignments = new();

        public MockClassroomService(SubtaskGeneratorService subtaskGenerator)
        {
            _subtaskGenerator = subtaskGenerator;
        }

        public List<CourseAssignment> GetAssignments() => _assignments;

        public Task RefreshAssignments()
        {
            var rawAssignments = new List<CourseAssignment>
            {
                new CourseAssignment
                {
                    Id = "1",
                    Title = "Math Final Project",
                    Subject = "Mathematics",
                    Description = "Complete the algebra chapter exercises and submit a summary report.",
                    Guidance = "Focus on chapters 5-8. Show all your working out.",
                    Deadline = "20/05/2026",
                    Score = 0,
                    Status = 1
                },
                new CourseAssignment
                {
                    Id = "2",
                    Title = "English Essay",
                    Subject = "English",
                    Description = "Write a 500 word essay on the themes of Shakespeare's Hamlet.",
                    Guidance = "Focus on the theme of revenge and its consequences.",
                    Deadline = "18/05/2026",
                    Score = 85,
                    Status = 3
                },
                new CourseAssignment
                {
                    Id = "3",
                    Title = "Science Lab Report",
                    Subject = "Biology",
                    Description = "Write up the results from the photosynthesis experiment.",
                    Guidance = "Include hypothesis, method, results and conclusion.",
                    Deadline = "22/05/2026",
                    Score = 0,
                    Status = 0
                },
                new CourseAssignment
                {
                    Id = "4",
                    Title = "History Presentation",
                    Subject = "History",
                    Description = "Prepare a 10 minute presentation on World War 2.",
                    Guidance = "Cover causes, key events and consequences.",
                    Deadline = "25/05/2026",
                    Score = 92,
                    Status = 3
                }
            };

            // Generate subtasks with fixed IDs
            foreach (var assignment in rawAssignments)
            {
                assignment.Subtasks = _subtaskGenerator
                    .GenerateSubtasks(assignment, assignment.Id);
            }

            _assignments = rawAssignments;
            return Task.CompletedTask;
        }

        List<CourseAssignment> IClassRooomService.GetAssignments()
        {
            throw new NotImplementedException();
        }
    }
}