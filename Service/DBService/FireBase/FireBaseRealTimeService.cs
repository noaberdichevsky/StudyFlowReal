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
        protected FirebaseClient? _firebaseClient;

        public FireBaseRealtimeService()
        {
            _firebaseClient = new FirebaseClient(
                "https://studyflowdb-default-rtdb.europe-west1.firebasedatabase.app/");
        }
    }
}
