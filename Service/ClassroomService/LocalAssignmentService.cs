using StudyFlow.Model;
using StudyFlow.Service.DBService.Firebase;

namespace StudyFlow.Service.ClassroomService
{
    public class LocalAssignmentService
    {
        private static List<CourseAssignment> _localAssignments = new();
        private static FireBaseLocalAssignmentRepository _repo = new();

        public static List<CourseAssignment> GetAll() => _localAssignments;

        public static async Task LoadFromFirebase()
        {
            _localAssignments = await _repo.LoadAssignmentsAsync();
            foreach (var assignment in _localAssignments)
                assignment.IsLocal = true;
        }

        public static async Task Add(CourseAssignment assignment)
        {
            assignment.IsLocal = true;
            _localAssignments.Add(assignment);
            await _repo.SaveAssignmentAsync(assignment);
        }

        public static async Task Remove(string id)
        {
            _localAssignments.RemoveAll(a => a.Id == id);
            await _repo.DeleteAssignmentAsync(id);
        }
        public static async Task UpdateAssignment(CourseAssignment assignment)
        {
            var index = _localAssignments.FindIndex(a => a.Id == assignment.Id);
            if (index >= 0)
                _localAssignments[index] = assignment;
            await _repo.SaveAssignmentAsync(assignment);
        }
    }
}