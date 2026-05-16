using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyFlow.Model
{
    public class Assignment
    {
        // Unique ID for this assignment in Firebase
        public string Id { get; set; } = string.Empty;

        // The assignment title e.g. "Math Homework"
        public string Title { get; set; } = string.Empty;

        // The subject e.g. "Mathematics", "English"
        public string Subject { get; set; } = string.Empty;

        // Full description of the assignment
        public string Description { get; set; } = string.Empty;

        // Guidance/hints from the teacher
        public string Guidance { get; set; } = string.Empty;

        // The deadline date as a string e.g. "20/05/2026"
        public string Deadline { get; set; } = string.Empty;

        // The score once graded - 0 means not graded yet
        public double Score { get; set; } = 0;

        // The current status of the assignment
        // 0 = Not Started, 1 = In Progress, 2 = Submitted, 3 = Graded
        public int Status { get; set; } = 0;

        // The list of subtasks for this assignment
        public List<SubTask> Subtasks { get; set; } = new();

        // Calculates the progress percentage based on completed subtasks
        // Returns a value between 0 and 1 (e.g. 0.5 = 50%)
        public double Progress
        {
            get
            {
                // If there are no subtasks return 0
                if (Subtasks == null || Subtasks.Count == 0)
                    return 0;

                // Count how many subtasks are completed
                var completed = Subtasks.Count(s => s.IsCompleted);

                // Divide completed by total to get percentage
                return (double)completed / Subtasks.Count;
            }
        }
    }
}