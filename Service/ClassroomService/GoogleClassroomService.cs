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
        //פונקציה קצרה שמחזירה את רשימת המטלות השמורה בזיכרון.

        public List<CourseAssignment> GetAssignments() => _assignments;

        public async Task RefreshAssignments()
        {
            try
            {
                //טוענת קודם את המטלות הידניות מ-Firebase לזיכרון.
                await LocalAssignmentService.LoadFromFirebase();
                //בודקת אם קיים טוקן של גוגל. אם לא — מציגה רק מטלות ידניות ומפסיקה.
                var token = await SecureStorage.Default.GetAsync("google_access_token");
                if (string.IsNullOrEmpty(token))
                {
                    _assignments = new List<CourseAssignment>(LocalAssignmentService.GetAll());
                    return;
                }
                //יוצרת חיבור HTTP ומוסיפה את הטוקן לכל בקשה.
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                //שולפת את כל הקורסים הפעילים של התלמיד מ-Google Classroom.
                var coursesResponse = await client.GetStringAsync(
                    "https://classroom.googleapis.com/v1/courses?courseStates=ACTIVE");
                //ממירה את התשובה ל-JSON. אם אין קורסים — מציגה רק מטלות ידניות ומפסיקה.
                var coursesDoc = JsonDocument.Parse(coursesResponse);
                if (!coursesDoc.RootElement.TryGetProperty("courses", out var courses))
                {
                    _assignments = new List<CourseAssignment>(LocalAssignmentService.GetAll());
                    return;
                }
                //יוצרת רשימה ריקה שתאסוף את כל המטלות.
                var assignments = new List<CourseAssignment>();
                //עוברת על כל קורס ושולפת את המזהה ושם הקורס.
                foreach (var course in courses.EnumerateArray())
                {
                    var courseId = course.GetProperty("id").GetString() ?? string.Empty;
                    var courseName = course.GetProperty("name").GetString() ?? string.Empty;

                    try
                    {
                        //שולפת את כל המטלות הפורסמות בקורס הנוכחי.
                        var worksResponse = await client.GetStringAsync(
                            $"https://classroom.googleapis.com/v1/courses/{courseId}/courseWork?courseWorkStates=PUBLISHED");

                        var worksDoc = JsonDocument.Parse(worksResponse);
                        //אם אין מטלות בקורס — מדלגת לקורס הבא.
                        if (!worksDoc.RootElement.TryGetProperty("courseWork", out var works))
                            continue;
                        //עוברת על כל מטלה ויוצרת אובייקט CourseAssignment עם הפרטים שהגיעו מגוגל.
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

                            try
                            {
                                //שולפת את הגשת התלמיד עבור המטלה הנוכחית.
                                var submissionsResponse = await client.GetStringAsync(
                                    $"https://classroom.googleapis.com/v1/courses/{courseId}/courseWork/{assignment.Id}/studentSubmissions?userId=me");

                                var submissionsDoc = JsonDocument.Parse(submissionsResponse);
                               
                                if (submissionsDoc.RootElement.TryGetProperty("studentSubmissions", out var submissions))
                                {
                                    var submission = submissions.EnumerateArray().FirstOrDefault();
                                    if (submission.ValueKind != JsonValueKind.Undefined)
                                    {
                                        // //אם יש ציון — שומרת אותו במטלה.
                                        if (submission.TryGetProperty("assignedGrade", out var grade))
                                            assignment.Score = grade.GetDouble();

                                        if (submission.TryGetProperty("state", out var state))
                                        {
                                            //ממירה את סטטוס ההגשה של גוגל למספר:
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
                            //יוצרת תת-משימות אוטומטיות לפי המקצוע ומוסיפה את המטלה לרשימה.
                            assignment.Subtasks = _subtaskGenerator.GenerateSubtasks(assignment, assignment.Id);
                            assignments.Add(assignment);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Course {courseId} error: {ex.Message}");
                    }
                }

                // הוסף מטלות ידניות
                assignments.AddRange(LocalAssignmentService.GetAll());

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
            //בודקת אם למטלה יש תאריך הגשה — לא לכל מטלה יש דדליין, לכן משתמשים ב-TryGetProperty שלא קורס אם לא קיים.
            if (work.TryGetProperty("dueDate", out var dueDate))
            {
                //שולפת את השנה, החודש והיום מה-JSON של גוגל — גוגל שולח את התאריך כשלושה מספרים נפרדים.
                var year = dueDate.GetProperty("year").GetInt32();
                var month = dueDate.GetProperty("month").GetInt32();
                var day = dueDate.GetProperty("day").GetInt32();
                //מחזירה את התאריך בפורמט dd/MM/yyyy — למשל 05/06/2026.
                return $"{day:D2}/{month:D2}/{year}";
            }
            return string.Empty;
        }
    }
}