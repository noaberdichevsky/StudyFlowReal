using StudyFlow.ViewModels;

namespace StudyFlow.Views
{
    public partial class SignInView : ContentPage
    {
        // Constructor receives SignInViewModel via Dependency Injection
        public SignInView(SignInViewModel vm)
        {
            InitializeComponent();

            // Pass the navigation object to the ViewModel
            vm.Navigation = this.Navigation;

            // Connect the ViewModel to this View
            BindingContext = vm;
        }

        // Called every time this page appears on screen
        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Tell the ViewModel the page appeared
            // so it can check for saved login (RememberMe)
            if (BindingContext is SignInViewModel vm)
                vm.OnAppearing();
        }
    }
}