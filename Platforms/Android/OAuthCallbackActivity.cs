using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace StudyFlow
{
    //לא נשמר בהיסטורית הניווט כשמשתמש לוחץ חזרה לא זוכרים את זה אם פתוח לא פותחים עוד עותק
    //מאפשר לאפליקציות חיצוניות לפתוח את האקטיביטי הזו — כמו Chrome שצריך לפתוח אותה אחרי שגוגל שולח את הקוד. בלי זה Chrome לא יוכל להפעיל אותה. 
    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
    //אומר לאנדרואיד כל אקטיביט שמתחילה בקידומת הזו תפתח את האטקיביטי הזה 
    [IntentFilter(
        new[] { Intent.ActionView },//מגיב לפתיחת URL
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },//קטגוריה ברירת מחדל ומאפשר לדפדפן לפתוח את האקטיביטי
        DataScheme = "com.companyname.studyflow")]//התכובת שמזהה את האפליקציה 
    //מחלקה שיורשת מ-Activity — זו נקודת הכניסה שמקבלת את ה-redirect מדף GitHub.
    public class OAuthCallbackActivity : Activity
    {
        //נקראת אוטומטית כשהאקטיביטי נפתחת. קוראת לפונקציית הבסיס תחילה.
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //מדפיסה ללוג את הכתובת המלאה שהגיעה — לצורכי דיבאג.
            System.Diagnostics.Debug.WriteLine($"OAuthCallback: {Intent?.Data}");
            //בודקת שהגיעה כתובת URL עם נתונים — אם לא, מדלגת על כל החילוץ.
            if (Intent?.Data != null)
            {
                //שולפת את הכתובת המלאה כמחרוזת — למשל:
                var url = Intent.Data.ToString()!;
                //ממירה את המחרוזת לאובייקט Uri שניתן לעבוד איתו בקלות.
                var uri = new System.Uri(url);
                //שולפת את החלק אחרי ? ומסירה את סימן השאלה — נשאר:
                var query = uri.Query.TrimStart('?');
                //מפצלת את הפרמטרים לפי & — למקרה שיש כמה פרמטרים.
                var parts = query.Split('&');
                //עוברת על כל פרמטר ומפצלת אותו לפי = — מקבלת מפתח וערך.
                foreach (var part in parts)
                {
                    var kv = part.Split('=');
                    //בודקת שהפרמטר הוא code — זה הקוד שצריך.
                    if (kv.Length == 2 && kv[0] == "code")
                    {
                        //שולפץ את הקוד ומפענחת תווים מיוחד
                        var code = System.Uri.UnescapeDataString(kv[1]);
                        //שומרת את הקוד ב-SecureStorage באופן סינכרוני — .GetAwaiter().GetResult() כי OnCreate אינה async.
                        SecureStorage.Default.SetAsync("oauth_code", code).GetAwaiter().GetResult();
                        //מדפיסה ללוג שהקוד נשמר ומפסיקה את הלולאה.
                        System.Diagnostics.Debug.WriteLine($"Code saved: {code}");
                        break;
                    }
                }
            }
            //פותחת את MainActivity חזרה עם שלושה דגלים: 
            //פותח בחלון חדש מנקה את כל האקטיביטי שמעל המיין לא פותח עותק חד
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            StartActivity(intent);
            Finish();
        }
    }
}