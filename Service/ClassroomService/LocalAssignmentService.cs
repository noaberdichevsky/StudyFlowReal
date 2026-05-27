using StudyFlow.Model;
using StudyFlow.Service.DBService.Firebase;

namespace StudyFlow.Service.ClassroomService
{
    public class LocalAssignmentService
    {
        private static List<CourseAssignment> _localAssignments = new();
        private static FireBaseLocalAssignmentRepository _repo = new();
        //פונקציה סטטית קצרה שמחזירה את רשימת המטלות הידניות מהזיכרון.
        public static List<CourseAssignment> GetAll() => _localAssignments;
        //פונקציה סטטית אסינכרונית שטוענת את המטלות הידניות מ-Firebase לזיכרון.
        public static async Task LoadFromFirebase()
        {
            //שולפת את כל המטלות הידניות מ-Firebase ושומרת אותן ברשימה בזיכרון.
            _localAssignments = await _repo.LoadAssignmentsAsync();
            //עוברת על כל המטלות שהגיעו מ-Firebase ומסמנת כל אחת כידנית עם IsLocal = true.
            foreach (var assignment in _localAssignments)
                assignment.IsLocal = true;
        }
        //פונקציה סטטית אסינכרונית שמקבלת מטלה חדשה ומוסיפה אותה.
        public static async Task Add(CourseAssignment assignment)
        {
            //מסמנת את המטלה כידנית — כדי שניתן יהיה למחוק אותה בעתיד.
            assignment.IsLocal = true;
            //מוסיפה את המטלה לרשימה בזיכרון מיידית — המשתמש רואה אותה על המסך מיד בלי לחכות לשמירה.
            _localAssignments.Add(assignment);
            //שומרת את המטלה ב-Firebase לצמיתות — כדי שתישמר גם אחרי סגירת האפליקציה.
            await _repo.SaveAssignmentAsync(assignment);
        }
        //פונקציה סטטית אסינכרונית שמקבלת מזהה מטלה ומוחקת אותה.
        public static async Task Remove(string id)
        {
            //מוחקת את המטלה מהרשימה בזיכרון מיידית — המשתמש רואה שהמטלה נעלמת מהמסך מיד.
          //  RemoveAll מוחקת את כל הפריטים שמתאימים לתנאי — במקרה זה כל מטלה שה - Id שלה שווה ל - id שהתקבל.
           _localAssignments.RemoveAll(a => a.Id == id);
            //מוחקת את המטלה מ-Firebase לצמיתות — כדי שלא תחזור בפעם הבאה שהאפליקציה נפתחת.
            await _repo.DeleteAssignmentAsync(id);
        }
        //פונקציה סטטית אסינכרונית שמקבלת מטלה מעודכנת ומחליפה את הישנה.
        public static async Task UpdateAssignment(CourseAssignment assignment)
        {
            //מחפשת את המיקום של המטלה ברשימה לפי ה-Id שלה — מחזירה מספר כמו 0, 1, 2... או 1- אם לא נמצאה.
            var index = _localAssignments.FindIndex(a => a.Id == assignment.Id);
            //אם המטלה נמצאה — מחליפה אותה בגרסה המעודכנת בזיכרון מיידית.
        //    index >= 0 בודקת שהמטלה אכן קיימת — כי FindIndex מחזיר 1 - אם לא נמצא.
            if (index >= 0)
                _localAssignments[index] = assignment;
            //שומרת את המטלה המעודכנת ב-Firebase לצמיתות.
            await _repo.SaveAssignmentAsync(assignment);
        }
    }
}