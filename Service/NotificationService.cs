using StudyFlow.Model;

namespace StudyFlow.Service
{
    public class NotificationService
    {
        private static HashSet<string> _shownNotifications = new();

        public static void ScheduleReminders(List<CourseAssignment> assignments)
        {
            foreach (var assignment in assignments)
            {
                if (string.IsNullOrEmpty(assignment.Deadline)) continue;

                if (DateTime.TryParseExact(assignment.Deadline, "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var deadline))
                {
                    var daysLeft = (deadline - DateTime.Today).TotalDays;

                    if (daysLeft == 7)
                        ShowAlert("Assignment Due in 1 Week",
                            $"{assignment.Title} is due in 7 days!");

                    if (daysLeft == 1)
                        ShowAlert("Assignment Due Tomorrow!",
                            $"{assignment.Title} is due tomorrow!");
                }
            }
        }

        private static void ShowAlert(string title, string message)
        {
            var key = title + message;
            if (_shownNotifications.Contains(key)) return;
            _shownNotifications.Add(key);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var page = Application.Current?.Windows[0]?.Page;
                if (page != null)
                    await page.DisplayAlert(title, message, "OK");
            });
        }
    }
}