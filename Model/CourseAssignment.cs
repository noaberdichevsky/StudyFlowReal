using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyFlow.Model
{
    public class CourseAssignment
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
        public bool IsLocal { get; set; } = false;

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
        // Returns color based on deadline proximity
        public Color DeadlineColor
        {
            get
            {
                if (string.IsNullOrEmpty(Deadline)) return Color.FromArgb("#5C6B8A");

                if (DateTime.TryParseExact(Deadline, "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var date))
                {
                    var daysLeft = (date - DateTime.Today).TotalDays;
                    if (daysLeft < 0) return Color.FromArgb("#922B21");      // עבר - אדום
                    if (daysLeft <= 3) return Color.FromArgb("#E67E22");     // קרוב - כתום
                    return Color.FromArgb("#1B8A4A");                        // בסדר - ירוק
                }
                return Color.FromArgb("#5C6B8A");
            }
        }
        public string DeadlineLabel
        {
            get
            {
                if (string.IsNullOrEmpty(Deadline)) return string.Empty;

                if (DateTime.TryParseExact(Deadline, "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var date))
                {
                    var daysLeft = (date - DateTime.Today).TotalDays;
                    if (daysLeft < 0) return $"{Deadline} (overdue)";
                    if (daysLeft == 0) return $"{Deadline} (today!)";
                    if (daysLeft == 1) return $"{Deadline} (1 day left)";
                    return $"{Deadline} ({(int)daysLeft} days left)";
                }
                return Deadline;
            }
        }
        public string StatusLabel
        {
            get => Status switch
            {
                2 => "✓ Submitted",
                3 => "★ Graded",
                1 => "In Progress",
                _ => ""
            };
        }

        public Color StatusColor
        {
            get => Status switch
            {
                2 => Color.FromArgb("#1B8A4A"),
                3 => Color.FromArgb("#C9A84C"),
                1 => Color.FromArgb("#5C6B8A"),
                _ => Colors.Transparent
            };
        }
    }
}