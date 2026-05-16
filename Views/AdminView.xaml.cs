using StudyFlow.ViewModels;

namespace StudyFlow.Views
{
    public partial class AdminView : ContentPage
    {
        // Constructor receives AdminViewModel via Dependency Injection
        public AdminView(AdminViewModel vm)
        {
            InitializeComponent();

            // Connect the ViewModel to this View
            BindingContext = vm;
        }
    }
}