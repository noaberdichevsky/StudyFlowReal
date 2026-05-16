using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Firebase.Database;
using Firebase.Database.Query;
using StudyFlow.Model;

namespace StudyFlow.Service.DBService.Firebase
{
    public class FireBaseProgressRepository : FireBaseRealtimeService, IProgressRepository
    {
        // Saves subtask progress to Firebase
        // Path: /progress/{userId}/{assignmentId}
        public async Task SaveProgressAsync(string userId, string assignmentId, List<SubTask> subtasks)
        {
            try
            {
                await _firebaseClient!
                    .Child("progress")      // go to progress node
                    .Child(userId)          // go to this user's node
                    .Child(assignmentId)    // go to this assignment's node
                    .PutAsync(subtasks);    // save the subtasks list
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveProgressAsync failed: {ex.Message}");
            }
        }

        // Loads subtask progress from Firebase
        // Returns null if no progress saved yet
        public async Task<List<SubTask>?> LoadProgressAsync(string userId, string assignmentId)
        {
            try
            {
                // Read the subtasks list from Firebase
                var subtasks = await _firebaseClient!
                    .Child("progress")
                    .Child(userId)
                    .Child(assignmentId)
                    .OnceSingleAsync<List<SubTask>>();

                return subtasks;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadProgressAsync failed: {ex.Message}");
                return null;
            }
        }
    }
}
