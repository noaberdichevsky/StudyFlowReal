using StudyFlow.ViewModels;

namespace StudyFlow.Views
{
    public partial class SignUpView : ContentPage
    {
        // Constructor receives SignUpViewModel via Dependency Injection
        public SignUpView(SignUpViewModel vm)
        {
            InitializeComponent();

            // Pass the navigation object to the ViewModel
            vm.Navigation = this.Navigation;

            // Connect the ViewModel to this View
            BindingContext = vm;
        }
    }
}