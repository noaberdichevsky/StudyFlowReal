using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StudyFlow.Model;

namespace StudyFlow.Service.ClassroomService
{
    // This interface defines what any classroom service must do
    // Whether mock data or real Google Classroom API
    // the rest of the app only talks to this interface, not the implementation
    public interface IClassRooomService
    {
        // Returns the current list of assignments
        List<CourseAssignment> GetAssignments();

        // Refreshes the assignments list
        // Called every time the main page loads
        Task RefreshAssignments();
    }
}