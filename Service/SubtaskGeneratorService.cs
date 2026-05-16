using StudyFlow.Model;

namespace StudyFlow.Service
{
    public class SubtaskGeneratorService
    {
        // Generates subtasks with fixed IDs based on assignmentId
        public List<SubTask> GenerateSubtasks(Assignment assignment, string assignmentId)
        {
            var subject = assignment.Subject.ToLower();

            if (subject.Contains("math") || subject.Contains("algebra") ||
                subject.Contains("calculus") || subject.Contains("geometry"))
                return GenerateMathSubtasks(assignmentId);

            else if (subject.Contains("english") || subject.Contains("literature") ||
                     subject.Contains("writing") || subject.Contains("essay"))
                return GenerateEnglishSubtasks(assignmentId);

            else if (subject.Contains("biology") || subject.Contains("chemistry") ||
                     subject.Contains("physics") || subject.Contains("science"))
                return GenerateScienceSubtasks(assignmentId);

            else if (subject.Contains("history") || subject.Contains("geography") ||
                     subject.Contains("social"))
                return GenerateHistorySubtasks(assignmentId);

            else if (subject.Contains("computer") || subject.Contains("programming") ||
                     subject.Contains("coding"))
                return GenerateCodingSubtasks(assignmentId);

            else
                return GenerateDefaultSubtasks(assignmentId);
        }

        private List<SubTask> GenerateMathSubtasks(string assignmentId)
        {
            return new List<SubTask>
            {
                new SubTask { Id = $"{assignmentId}_1", Title = "Review relevant theory and formulas", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_2", Title = "Read the assignment instructions carefully", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_3", Title = "Complete practice problems", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_4", Title = "Check all calculations and working", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_5", Title = "Write up final solution", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_6", Title = "Submit on Google Classroom", IsCompleted = false }
            };
        }

        private List<SubTask> GenerateEnglishSubtasks(string assignmentId)
        {
            return new List<SubTask>
            {
                new SubTask { Id = $"{assignmentId}_1", Title = "Read all required material", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_2", Title = "Take notes on key themes and ideas", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_3", Title = "Plan essay structure and outline", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_4", Title = "Write first draft", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_5", Title = "Proofread and edit", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_6", Title = "Submit on Google Classroom", IsCompleted = false }
            };
        }

        private List<SubTask> GenerateScienceSubtasks(string assignmentId)
        {
            return new List<SubTask>
            {
                new SubTask { Id = $"{assignmentId}_1", Title = "Read relevant chapter and notes", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_2", Title = "Research the topic", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_3", Title = "Write hypothesis or introduction", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_4", Title = "Complete the main work", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_5", Title = "Write conclusion and summary", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_6", Title = "Submit on Google Classroom", IsCompleted = false }
            };
        }

        private List<SubTask> GenerateHistorySubtasks(string assignmentId)
        {
            return new List<SubTask>
            {
                new SubTask { Id = $"{assignmentId}_1", Title = "Research the topic thoroughly", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_2", Title = "Take notes on key events and dates", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_3", Title = "Plan your response structure", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_4", Title = "Write your response", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_5", Title = "Review and edit", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_6", Title = "Submit on Google Classroom", IsCompleted = false }
            };
        }

        private List<SubTask> GenerateCodingSubtasks(string assignmentId)
        {
            return new List<SubTask>
            {
                new SubTask { Id = $"{assignmentId}_1", Title = "Read and understand requirements", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_2", Title = "Plan the solution and structure", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_3", Title = "Write the code", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_4", Title = "Test and debug", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_5", Title = "Write documentation/comments", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_6", Title = "Submit on Google Classroom", IsCompleted = false }
            };
        }

        private List<SubTask> GenerateDefaultSubtasks(string assignmentId)
        {
            return new List<SubTask>
            {
                new SubTask { Id = $"{assignmentId}_1", Title = "Read and understand the assignment", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_2", Title = "Research and gather information", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_3", Title = "Plan your approach", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_4", Title = "Complete the main work", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_5", Title = "Review and check everything", IsCompleted = false },
                new SubTask { Id = $"{assignmentId}_6", Title = "Submit on Google Classroom", IsCompleted = false }
            };
        }
    }
}