using StudyFlow.Model;

namespace StudyFlow.Service.DBService
{
    public interface IProgressRepository
    {
        // Save subtask progress for an assignment
        Task SaveProgressAsync(string userId, string assignmentId, List<SubTask> subtasks);

        // Load subtask progress for an assignment
        // Returns null if no progress saved yet
        Task<List<SubTask>?> LoadProgressAsync(string userId, string assignmentId);
    }
}