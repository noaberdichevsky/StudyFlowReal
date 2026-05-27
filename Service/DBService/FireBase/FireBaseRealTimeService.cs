using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Firebase.Database;

namespace StudyFlow.Service.DBService.Firebase
{
    public class FireBaseRealtimeService
    {
        //משתנה שמאחסן את החיבור ל-Firebase: נגיש רק למחלקה הזו ולמחלקות היורשות ממנה הסימן ? אומר שיכול להיות null  שם המשתנה לפי מוסכמה של משתנים פרטיים
        protected FirebaseClient? _firebaseClient;

        public FireBaseRealtimeService()
        {
            _firebaseClient = new FirebaseClient(
                "https://studyflowdb-default-rtdb.europe-west1.firebasedatabase.app/");//יוצרת חיבור ל-Firebase עם כתובת בסיס הנתונים.
        }
    }
}
