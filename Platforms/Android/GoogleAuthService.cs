using Android.Content;
using StudyFlow.Service;
using System.Security.Cryptography;
using System.Text;

namespace StudyFlow
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private static string _codeVerifier = string.Empty;

        private static string GenerateCodeVerifier()
        {
            //מערך חדש של 32 בתים
            var bytes = new byte[32];
            //ממלא במספרים רנדומלים
            RandomNumberGenerator.Fill(bytes);
            //ממיר מחרוזת לטקסט קריא ומחליפה סימנים לא חוקיים 
            return Convert.ToBase64String(bytes)
                .Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        private static string GenerateCodeChallenge(string verifier)
        {
            //מחזירה מחרוזת למערך בתים ומצפינה בעזרת שה 
            var hash = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));
            //ממיר למחרוזת טקסט ואז מחליף תווים לא חוקים 
            return Convert.ToBase64String(hash)
                .Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        private static async Task<string> ExchangeCodeForToken(string code)
        {
            //יוצר חיבור HTTP ומבטיחה שזה יסגר אוטומטית בסוף הפונקציה
            using var client = new HttpClient();
            //יוצר תוכן בקשה לגוגל כמו טופס עם שדות קוד חד פעמי, מזהה האפליקציה של גוגל סיסמה סודית של האפליקציה 
            //אותו דף שהגדרתי בגיטהאב אומר לגוגל שאנחנו מחליפים קוד בטוקן והוירפייר שיצרו לאימות אבטחה
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", "842354198030-9geod1hq8c9eq5kgnsdbc7kn6pnimain.apps.googleusercontent.com"),
                new KeyValuePair<string, string>("client_secret", "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"),
                new KeyValuePair<string, string>("redirect_uri", "https://noaberdichevsky.github.io/studyflow-auth/auth.html"),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code_verifier", _codeVerifier),
            });
            //שולחת את הבקשה לגוגל ומחכה לתשובה.
            var response = await client.PostAsync("https://oauth2.googleapis.com/token", content);
            //קוראת את התשובה של גוגל כטקסט JSON.
            var json = await response.Content.ReadAsStringAsync();
            //מדפיסה את התשובה לחלון ה-Output של Visual Studio — לצורכי דיבאג בלבד.
            System.Diagnostics.Debug.WriteLine($"Token response: {json}");
            //ממירה את ה-JSON לאובייקט שניתן לעבוד איתו.
            var tokenData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
            //מנסה לשלוף את ה-access_token מהתשובה — אם קיים מחזירה אותו, אם לא ממשיכה לשורה הבאה.
            if (tokenData.TryGetProperty("access_token", out var accessToken))
                return accessToken.GetString() ?? string.Empty;
            //אם לא התקבל טוקן — מחזירה מחרוזת ריקה, כלומר הכניסה נכשלה. 
            return string.Empty;
        }

        public async Task<string> SignInAsync()
        {
            //מוחקת קוד ישן מהאחסון המוצפן לפני התחלת תהליך חדש — מונעת שימוש בקוד פג תוקף מהפעלה קודמת.
            SecureStorage.Default.Remove("oauth_code");
            //יוצרת את שני קודי האבטחה — ה-Verifier נשאר באפליקציה וה-Challenge נשלח לגוגל.
            _codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GenerateCodeChallenge(_codeVerifier);
            //בנה את כתובת הבקשה עם כל הפרטים מזהה האפליקציה דך הגיט בקשת קוד ולא טוקן
            //הרשאות קוד אבטחה מוצפן אלגוריתם הצפנה מראה למתמש בחירת החשבון 
            var url = "https://accounts.google.com/o/oauth2/v2/auth" +
                "?client_id=842354198030-9geod1hq8c9eq5kgnsdbc7kn6pnimain.apps.googleusercontent.com" +
                "&redirect_uri=https%3A%2F%2Fnoaberdichevsky.github.io%2Fstudyflow-auth%2Fauth.html" +
                "&response_type=code" +
                "&scope=email%20profile%20https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fclassroom.courses.readonly%20https%3A%2F%2Fwww" +
                ".googleapis.com%2Fauth%2Fclassroom.coursework.me.readonly%20https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fclassroom.coursework.students" +
                "&code_challenge=" + codeChallenge +
                "&code_challenge_method=S256" +
                "&prompt=select_account";
            //פותחת את Chrome עם כתובת גוגל. NewTask אומר לפתוח בחלון חדש
            var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity!;
            var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(url));
            intent.AddFlags(ActivityFlags.NewTask);
            activity.StartActivity(intent);
            //מגדירה timeout של 3 דקות — אם לא הגיע קוד תוך 3 דקות הכניסה נכשלת.
            var timeout = Task.Delay(180000);
            //לולאה שבודקת כל שנייה אם הגיע קוד — ממשיכה עד שה-timeout מסתיים
            while (!timeout.IsCompleted)
            {
                await Task.Delay(1000);
                try
                {
                    //בודק אם סיקיור סטורג שמר את  הקוד במיין אקטיביטי ומדפיסה את ערך הלוג
                    var code = await SecureStorage.Default.GetAsync("oauth_code");
                    //אם נמצא קוד תקין — מוחקת אותו מה-SecureStorage ושולחת אותו ל-ExchangeCodeForToken לקבלת טוקן.
                    System.Diagnostics.Debug.WriteLine($"SecureStorage code: '{code}'");
                    if (!string.IsNullOrEmpty(code) && code.Length > 10)
                    {
                        SecureStorage.Default.Remove("oauth_code");
                        return await ExchangeCodeForToken(code);
                    }
                }
                catch { }
            }

            return string.Empty;
        }
    }
}