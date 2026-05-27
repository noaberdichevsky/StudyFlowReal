using Android.App;
using Android.Content;
using Android.Content.PM;

namespace StudyFlow
{
    [Activity(Theme = "@style/Maui.SplashTheme",
               MainLauncher = true,
               LaunchMode = LaunchMode.SingleTop,
               ConfigurationChanges = ConfigChanges.ScreenSize |
                                      ConfigChanges.Orientation |
                                      ConfigChanges.UiMode)]
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "studyflowauth",
        DataHost = "callback"
    )]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            HandleIntent(intent);
        }

        protected override void OnResume()
        {
            base.OnResume();
            HandleIntent(Intent);
        }
        //פונקציה פרטית שמקבלת Intent ומחלצת ממנו את קוד ה-OAuth.
        private void HandleIntent(Intent? intent)
        {
            //שולפת את הכתובת מה-Intent — אם ה-Intent או ה-Data הם null מחזירה null בלי לקרוס.
            var url = intent?.Data?.ToString();
            //מדפיסה את הכתובת ללוג — לצורכי דיבאג.
            System.Diagnostics.Debug.WriteLine($"HandleIntent: {url}");
            //בודקת שתי תנאים:

      //      הכתובת קיימת ולא ריקה
//הכתובת מכילה את המילה code = — אחרת אין טעם להמשיך
            if (!string.IsNullOrEmpty(url) && url.Contains("code="))
            {
                //ממירה את הכתובת לאובייקט Uri ושולפת את החלק אחרי ? — למשל code=abc123.
                var uri = new Uri(url);
                var query = uri.Query.TrimStart('?');
                //מפצלת את הפרמטרים לפי & ועוברת על כל אחד.
                foreach (var part in query.Split('&'))
                {
                    //מפצלת כל פרמטר לפי = ובודקת שהמפתח הוא code.
                    var kv = part.Split('=');
                    if (kv.Length == 2 && kv[0] == "code")
                    {
                        //שולפת את הקוד ומפענחת תווין מיוחדים 
                        var code = Uri.UnescapeDataString(kv[1]);
                        //מדפיסה את הקוד ללוג לאישור שנמצא.
                        System.Diagnostics.Debug.WriteLine($"Got code: {code}");
                        //מוחקת את הנתונים מה-Intent כדי שלא יטופל פעמיים — אם OnResume תיקרא שוב לא תמצא קוד.
                        intent!.SetData(null);
                        //שומרת את הקוד ב-SecureStorage באופן סינכרוני — .Wait() כי הפונקציה לא async. GoogleAuthService בודק אותו כל שנייה ומוצא אותו כאן.
                        SecureStorage.Default.SetAsync("oauth_code", code).Wait();
                        break;
                    }
                }
            }
        }
    }
}