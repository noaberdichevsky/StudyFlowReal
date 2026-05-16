using StudyFlow.ViewModels;

namespace StudyFlow.Views
{
    public partial class AssignmentDetailView : ContentPage
    {
        // Constructor receives AssignmentDetailViewModel via Dependency Injection
        public AssignmentDetailView(AssignmentDetailViewModel vm)
        {
            InitializeComponent();

            // Connect the ViewModel to this View
            BindingContext = vm;
        }
    }
}