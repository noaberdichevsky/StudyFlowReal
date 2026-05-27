using Firebase.Auth;
using Firebase.Auth.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyFlow.Service.DBService.FireBase
{
    internal class FireBaseAuthService:IAuthService
    {
        private FirebaseAuthClient? _authClient;
        private IAppLogger _logger;
        //קונסטרוקטור שמקבל את שירות הלוגים בהזרקת תלויות.
        public FireBaseAuthService(IAppLogger logger)
        {
            //שומר את שירות הלוגים לשימוש בכל הפונקציות.
            _logger = logger;
            //יוצרת אובייקט הגדרות לחיבור ל-Firebase Authentication.
            var config = new FirebaseAuthConfig()
            {
                // current_api_key from google-services.json
                ApiKey = "AIzaSyCXcogsbrbnmObz0i4tgKtxD5XVwGVechw",

                // project_id from google-services.json + ".firebaseapp.com"
                AuthDomain = "studyflowdb.firebaseapp.com",
                //מגדירה אילו שיטות כניסה מותרות — במקרה זה אימייל וסיסמה בלבד. אפשר להוסיף GoogleProvider, FacebookProvider וכו'.
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                },
            };
            //יוצרת את לקוח האימות עם ההגדרות שהוגדרו — ישמש לכל פעולות האימות כמו SignIn, CreateAuth ו-RemoveAuth. 
            _authClient = new FirebaseAuthClient(config);
        }
        //פונקציה שמקבלת אימייל וסיסמה ומחזירה את ה-UID של המשתמש אחרי כניסה מוצלחת.
        public async Task<string> SignIn(string userEmail, string userPassword)
        {
            //משתנה שישמור את הודעת השגיאה אם תהיה — מאותחל כמחרוזת ריקה.
            string errorMessage = string.Empty;
            try
            {
                //שולחת בקשת כניסה ל-Firebase עם האימייל והסיסמה.
                await _authClient!.SignInWithEmailAndPasswordAsync(userEmail, userPassword);
                //אם הכניסה הצליחה — מחזירה את ה-UID הייחודי של המשתמש.
                return _authClient.User.Info.Uid;
            }
            catch (FirebaseAuthException ex)
            {
                //תופסת שגיאות ספציפיות של Firebase:
                if (ex.Message.Contains("INVALID_LOGIN_CREDENTIALS"))
                    errorMessage = "Incorrect email or password!";
                else
                    errorMessage = "SignIn failed: Unknown exception!";
                //מתעדת את השגיאה ללוג וזורקת אותה ל-ViewModel כדי שיציג למשתמש.
                _logger.LogDebug($"SignIn failed: {userEmail}, {errorMessage}");
                throw new Exception(errorMessage);
            }
            //תופסת שגיאות בלתי צפויות כמו בעיית רשת ומחזירה הודעה כללית.
            catch (Exception ex)
            {
                _logger.LogDebug($"SignIn failed: {ex.Message}");
                throw new Exception("SignIn failed!");
            }
        }
        //פונקציה שמקבלת אימייל וסיסמה ויוצרת חשבון חדש ב-Firebase Authentication.
        public async Task<string> CreateAuth(string userEmail, string userPassword)
        {
            try
            {
                //פונקציה שמקבלת אימייל וסיסמה ויוצרת חשבון חדש ב-Firebase Authentication.
                await _authClient!.CreateUserWithEmailAndPasswordAsync(userEmail, userPassword);
                //מתעדת ללוג שהיצירה הצליחה ומחזירה את ה-UID הייחודי שגוגל יצר למשתמש — ישמש כמפתח ב-Firebase.
                _logger.LogDebug($"User {userEmail} created successfully");
                return _authClient.User.Uid;
            }
            //תופסת שגיאות ספציפיות של Firebase Authentication — כמו אימייל שגוי או סיסמה חלשה. 
            catch (FirebaseAuthException ex)
            {
                string errorMessage = string.Empty;
                //בודקת איזו שגיאה קרתה ומתרגמת אותה להודעה ברורה למשתמש:
                if (ex.Message.Contains("INVALID_EMAIL"))
                    errorMessage = "Invalid email address!";
                else if (ex.Message.Contains("EMAIL_EXISTS"))
                    errorMessage = "This email already exists!";
                else if (ex.Message.Contains("WEAK_PASSWORD"))
                    errorMessage = "Weak password!";
                //מתעדת את השגיאה ללוג וזורקת שגיאה עם ההודעה הברורה כדי שה-ViewModel יציג אותה למשתמש.
                _logger.LogDebug($"CreateAuth failed: {ex.Message}");
                throw new Exception(errorMessage);
            }
            //תופסת כל שגיאה אחרת בלתי צפויה — כמו בעיית רשת — וזורקת הודעה כללית. 
            catch (Exception ex)
            {
                _logger.LogDebug($"CreateAuth failed: {ex.Message}");
                throw new Exception("SignUp failed!");
            }
        }
        //פונקציה שמקבלת אימייל וסיסמה ומוחקת את חשבון המשתמש מ-Firebase Authentication.
        public async Task RemoveAuth(string userEmail, string userPassword)
        {
            try
            {
                //מבצעת אימות מחדש של המשתמש לפני המחיקה — Firebase דורש זאת מטעמי אבטחה. אם הסיסמה שגויה — המחיקה לא תתבצע.
                await _authClient!.SignInWithEmailAndPasswordAsync(userEmail, userPassword);
                //מוחקת את חשבון המשתמש מ-Firebase Authentication לצמיתות.
                await _authClient.User.DeleteAsync();
                //מדפיסה ללוג שהמחיקה הצליחה.
                _logger.LogDebug($"User {userEmail} removed successfully");
            }
            //אם קרתה שגיאה — מתעדת אותה ללוג וזורקת שגיאה חדשה עם הודעה ברורה.
            catch (Exception ex)
            {
                _logger.LogDebug($"RemoveAuth failed: {ex.Message}");
                throw new Exception("Remove user failed!");
            }
        }

        public Task SignOut()
        {
            throw new NotImplementedException();
        }
    }
}


