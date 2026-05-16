using StudyFlow.Model;
using StudyFlow.Views;

namespace StudyFlow
{
    public partial class App : Application
    {
        // CurrentUser holds the logged in user for the whole app lifetime
        public AppUser? CurrentUser { get; set; }

        public App(SignInView signInView)
        {
            InitializeComponent();

            // Start the app on the SignIn page wrapped in a NavigationPage
            MainPage = new NavigationPage(signInView);
        }
    }
}