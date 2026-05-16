using StudyFlow.Model;
using System.Net.Http.Headers;
using System.Text.Json;

namespace StudyFlow.Service.ClassroomService
{
    public class GoogleClassroomService : IClassRooomService
    {
        private readonly SubtaskGeneratorService _subtaskGenerator;
        private List<CourseAssignment> _assignments = new();

        public GoogleClassroomService(SubtaskGeneratorService subtaskGenerator)
        {
            _subtaskGenerator = subtaskGenerator;
        }

        public List<CourseAssignment> GetAssignments() => _assignments;

        public async Task RefreshAssignments()
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync("google_access_token");
                System.Diagnostics.Debug.WriteLine($"Token in classroom: {token}");
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("No token found!");
                    return;
                }

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var coursesResponse = await client.GetStringAsync(
                    "https://classroom.googleapis.com/v1/courses?courseStates=ACTIVE");

                var coursesDoc = JsonDocument.Parse(coursesResponse);
                if (!coursesDoc.RootElement.TryGetProperty("courses", out var courses))
                {
                    System.Diagnostics.Debug.WriteLine("No courses found!");
                    return;
                }

                var assignments = new List<CourseAssignment>();

                foreach (var course in courses.EnumerateArray())
                {
                    var courseId = course.GetProperty("id").GetString() ?? string.Empty;
                    var courseName = course.GetProperty("name").GetString() ?? string.Empty;
                    System.Diagnostics.Debug.WriteLine($"Course: {courseName}");

                    try
                    {
                        var worksResponse = await client.GetStringAsync(
                            $"https://classroom.googleapis.com/v1/courses/{courseId}/courseWork?courseWorkStates=PUBLISHED");

                        var worksDoc = JsonDocument.Parse(worksResponse);
                        if (!worksDoc.RootElement.TryGetProperty("courseWork", out var works))
                            continue;

                        foreach (var work in works.EnumerateArray())
                        {
                            var assignment = new CourseAssignment
                            {
                                Id = work.TryGetProperty("id", out var id) ? id.GetString() ?? string.Empty : string.Empty,
                                Title = work.TryGetProperty("title", out var title) ? title.GetString() ?? string.Empty : string.Empty,
                                Subject = courseName,
                                Description = work.TryGetProperty("description", out var desc) ? desc.GetString() ?? string.Empty : string.Empty,
                                Guidance = string.Empty,
                                Deadline = GetDeadline(work),
                                Score = 0,
                                Status = 0
                            };

                            // קבל ציון וסטטוס
                            try
                            {
                                var submissionsResponse = await client.GetStringAsync(
                                    $"https://classroom.googleapis.com/v1/courses/{courseId}/courseWork/{assignment.Id}/studentSubmissions?userId=me");

                                var submissionsDoc = JsonDocument.Parse(submissionsResponse);
                                if (submissionsDoc.RootElement.TryGetProperty("studentSubmissions", out var submissions))
                                {
                                    var submission = submissions.EnumerateArray().FirstOrDefault();
                                    if (submission.ValueKind != JsonValueKind.Undefined)
                                    {
                                        if (submission.TryGetProperty("assignedGrade", out var grade))
                                            assignment.Score = grade.GetDouble();

                                        if (submission.TryGetProperty("state", out var state))
                                        {
                                            assignment.Status = state.GetString() switch
                                            {
                                                "TURNED_IN" => 2,
                                                "RETURNED" => 3,
                                                "CREATED" => 0,
                                                _ => 0
                                            };
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Submission error: {ex.Message}");
                            }

                            assignment.Subtasks = _subtaskGenerator.GenerateSubtasks(assignment, assignment.Id);
                            assignments.Add(assignment);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Course {courseId} error: {ex.Message}");
                    }
                }

                _assignments = assignments;
                System.Diagnostics.Debug.WriteLine($"Total assignments: {_assignments.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Classroom error: {ex.Message}");
            }
        }

        private string GetDeadline(JsonElement work)
        {
            if (work.TryGetProperty("dueDate", out var dueDate))
            {
                var year = dueDate.GetProperty("year").GetInt32();
                var month = dueDate.GetProperty("month").GetInt32();
                var day = dueDate.GetProperty("day").GetInt32();
                return $"{day:D2}/{month:D2}/{year}";
            }
            return string.Empty;
        }
    }
}