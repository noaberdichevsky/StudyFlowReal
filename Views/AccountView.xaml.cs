using StudyFlow.ViewModels;

namespace StudyFlow.Views
{
    public partial class AccountView : ContentPage
    {
        // Constructor receives AccountViewModel via Dependency Injection
        public AccountView(AccountViewModel vm)
        {
            InitializeComponent();

            // Connect the ViewModel to this View
            BindingContext = vm;
        }
    }
}