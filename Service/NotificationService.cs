using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudyFlow.Model;

namespace StudyFlow.Service
{
    public class NotificationService
    {
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
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var page = Application.Current?.Windows[0]?.Page;
                if (page != null)
                    await page.DisplayAlert(title, message, "OK");
            });
        }
    }
}