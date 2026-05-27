using Firebase.Database;
using Firebase.Database.Query;
using StudyFlow.Model;

namespace StudyFlow.Service.DBService.Firebase
{
    public class FireBaseLocalAssignmentRepository
    {
        private readonly FirebaseClient _firebaseClient;
        private readonly string _userId;

        public FireBaseLocalAssignmentRepository()
        {
            //יוצרת חיבור ל-Firebase עם כתובת בסיס הנתונים.
            _firebaseClient = new FirebaseClient(
                "https://studyflowdb-default-rtdb.europe-west1.firebasedatabase.app/");
            //שולפת את ה-Id של המשתמש המחובר כרגע — שלושה חלקים:
            //App.Current as App — מקבלת את האפליקציה הנוכחית
            //?.CurrentUser?.Id — שולפת את ה-Id בבטחה עם הגנה מ-null
            //?? string.Empty — אם ה-Id הוא null מחזירה מחרוזת ריקה
            _userId = (App.Current as App)?.CurrentUser?.Id ?? string.Empty;
        }

        public async Task SaveAssignmentAsync(CourseAssignment assignment)
        {
            //שומרת מטלה ידנית ב-Firebase תחת הנתיב:
            //localAssignments → userId → assignmentId
            //PutAsync מחליף את כל הנתונים הקיימים בנתיב זה — אם המטלה קיימת היא תתעדכן, אם לא היא תיווצר. 
            await _firebaseClient
                .Child("localAssignments")
                .Child(_userId)
                .Child(assignment.Id)
                .PutAsync(assignment);
        }
        //פונקציה אסינכרונית שמחזירה רשימת מטלות ידניות מ-Firebase.
        public async Task<List<CourseAssignment>> LoadAssignmentsAsync()
        {
            try
            {
                //שולפת את כל המטלות של המשתמש מהנתיב:
                //localAssignments → userId
                //OnceSingleAsync שולף פעם אחת בלבד. התוצאה היא Dictionary כי Firebase שומר נתונים כמפתח-ערך.
                var assignments = await _firebaseClient
                    .Child("localAssignments")
                    .Child(_userId)
                    .OnceSingleAsync<Dictionary<string, CourseAssignment>>();
                //אם אין מטלות בכלל — מחזירה רשימה ריקה בלי לקרוס.
                if (assignments == null) return new List<CourseAssignment>();
                //מחלצת רק את הערכים מה-Dictionary ומחזירה אותם כרשימה רגילה.
                return assignments.Values.ToList();
            }
            //אם קרתה שגיאה כלשהי — מחזירה רשימה ריקה בלי לקרוס.
            catch
            {
                return new List<CourseAssignment>();
            }
        }
        //פונקציה אסינכרונית שמקבלת מזהה מטלה ומוחקת אותה מ-Firebase.
        public async Task DeleteAssignmentAsync(string assignmentId)
        {
            //משתמשת בחיבור ל-Firebase שנוצר בקונסטרוקטור.
            //נכנסת לנתיב localAssignments בבסיס הנתונים.
            //נכנסת לתיקיית המשתמש הנוכחי — כך כל משתמש רואה רק את המטלות שלו.
            //נכנסת למטלה הספציפית לפי המזהה שלה.
            //מוחקת את המטלה מ-Firebase לצמיתות.
            await _firebaseClient
                .Child("localAssignments")
                .Child(_userId)
                .Child(assignmentId)
                .DeleteAsync();
        }
    }
}