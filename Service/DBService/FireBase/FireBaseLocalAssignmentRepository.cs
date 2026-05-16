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
            _firebaseClient = new FirebaseClient(
                "https://studyflowdb-default-rtdb.europe-west1.firebasedatabase.app/");
            _userId = (App.Current as App)?.CurrentUser?.Id ?? string.Empty;
        }

        public async Task SaveAssignmentAsync(CourseAssignment assignment)
        {
            await _firebaseClient
                .Child("localAssignments")
                .Child(_userId)
                .Child(assignment.Id)
                .PutAsync(assignment);
        }

        public async Task<List<CourseAssignment>> LoadAssignmentsAsync()
        {
            try
            {
                var assignments = await _firebaseClient
                    .Child("localAssignments")
                    .Child(_userId)
                    .OnceSingleAsync<Dictionary<string, CourseAssignment>>();

                if (assignments == null) return new List<CourseAssignment>();
                return assignments.Values.ToList();
            }
            catch
            {
                return new List<CourseAssignment>();
            }
        }

        public async Task DeleteAssignmentAsync(string assignmentId)
        {
            await _firebaseClient
                .Child("localAssignments")
                .Child(_userId)
                .Child(assignmentId)
                .DeleteAsync();
        }
    }
}