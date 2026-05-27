using StudyFlow.Model;

namespace StudyFlow.Service
{
    public class NotificationService
    {
        private static HashSet<string> _shownNotifications = new();

        public static void ScheduleReminders(List<CourseAssignment> assignments)
        {
            //עוברת על כל מטלה ברשימה.
            foreach (var assignment in assignments)
            {
                //אם למטלה אין תאריך הגשה — מדלגת עליה וממשיכה לבאה.
                if (string.IsNullOrEmpty(assignment.Deadline)) continue;
                //מנסה להמיר את תאריך ההגשה מ-string לתאריך אמיתי בפורמט dd/MM/yyyy. אם ההמרה נכשלת — מדלגת.
                if (DateTime.TryParseExact(assignment.Deadline, "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var deadline))
                {
                    //מחשבת כמה ימים נותרו עד מועד ההגשה — מחסירה את היום הנוכחי מתאריך ההגשה.
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
        //פונקציה פרטית וסטטית שמציגה התראה למשתמש — נקראת מ
        private static void ShowAlert(string title, string message)
        {
            //יוצרת מפתח ייחודי מצירוף הכותרת וההודעה — למשל:
            var key = title + message;
            //בודקת אם ההתראה הזו כבר הוצגה — אם כן יוצאת מהפונקציה מיד. מונעת הצגה כפולה.
            if (_shownNotifications.Contains(key)) return;
            //מוסיפה את המפתח לרשימה כדי שלא יוצג שוב באותה הפעלה.
            _shownNotifications.Add(key);
            //מריצה את הקוד שבפנים על ה-Thread הראשי — חובה כי עדכוני ממשק חייבים לרוץ על ה-Thread הראשי בלבד. בלי זה האפליקציה קורסת.

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                //שולפת את המסך הנוכחי של האפליקציה — עם הגנה מ-null בכל שלב.
                var page = Application.Current?.Windows[0]?.Page;
                //אם יש מסך פעיל — מציגה את ההתראה עם כפתור אישור. 😊
                if (page != null)
                    await page.DisplayAlert(title, message, "OK");
            });
        }
    }
}