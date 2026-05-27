using StudyFlow.Model;
using StudyFlow.Service.DBService.Firebase;

namespace StudyFlow.Service.ClassroomService
{
    public class LocalAssignmentService
    {
        // רשימת המטלות הידניות בזיכרון — static כי משותפת לכל האפליקציה
        private static List<CourseAssignment> _localAssignments = new();

        // החיבור ל-Firebase — static כי נוצר פעם אחת
        // בעיה: שומר את userId של המשתמש הראשון ולא מתעדכן
        private static FireBaseLocalAssignmentRepository _repo = new();

        // מחזיר את רשימת המטלות מהזיכרון
        public static List<CourseAssignment> GetAll() => _localAssignments;

        // פונקציה חדשה — מאפסת את כל הנתונים ביציאה מהחשבון
        public static void Clear()
        {

            // מאפסת את רשימת המטלות בלבד
            _localAssignments = new List<CourseAssignment>();
        }

        // טוענת את המטלות מ-Firebase לזיכרון
        public static async Task LoadFromFirebase()
        {
            // יוצרת _repo חדש עם המשתמש שכבר מחובר
            _repo = new FireBaseLocalAssignmentRepository();
            // שולפת את כל המטלות של המשתמש מ-Firebase
            _localAssignments = await _repo.LoadAssignmentsAsync();

            // מסמנת כל מטלה כידנית
            foreach (var assignment in _localAssignments)
                assignment.IsLocal = true;
        }

        // מוסיפה מטלה חדשה לזיכרון ול-Firebase
        public static async Task Add(CourseAssignment assignment)
        {
            // מסמנת את המטלה כידנית
            assignment.IsLocal = true;

            // מוסיפה לזיכרון מיידית — המשתמש רואה מיד
            _localAssignments.Add(assignment);

            // שומרת ב-Firebase לצמיתות
            await _repo.SaveAssignmentAsync(assignment);
        }

        // מוחקת מטלה מהזיכרון ומ-Firebase
        public static async Task Remove(string id)
        {
            // מוחקת מהזיכרון מיידית
            _localAssignments.RemoveAll(a => a.Id == id);

            // מוחקת מ-Firebase לצמיתות
            await _repo.DeleteAssignmentAsync(id);
        }

        // מעדכנת מטלה קיימת בזיכרון ול-Firebase
        public static async Task UpdateAssignment(CourseAssignment assignment)
        {
            // מוצאת את המיקום של המטלה ברשימה
            var index = _localAssignments.FindIndex(a => a.Id == assignment.Id);

            // אם נמצאה — מחליפה אותה בגרסה המעודכנת
            if (index >= 0)
                _localAssignments[index] = assignment;

            // שומרת את הגרסה המעודכנת ב-Firebase
            await _repo.SaveAssignmentAsync(assignment);
        }
    }
}