using StudyFlow.ViewModels;

namespace StudyFlow
{
    public partial class AppShell : Shell
    {
        public AppShell(AppShellViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;

            // Register routes for pages not in the tab bar
            Routing.RegisterRoute("UsersListView", typeof(Views.UsersListView));
            Routing.RegisterRoute("AssignmentDetailView", typeof(Views.AssignmentDetailView));
            Routing.RegisterRoute("AddAssignmentView", typeof(Views.AddAssignmentView));
            Routing.RegisterRoute("UserProfileView", typeof(Views.UserProfileView));

            // Hide admin tab for regular users
            var currentUser = (App.Current as App)?.CurrentUser;
            if (currentUser != null && !currentUser.IsAdmin)
            {
                var adminTab = this.Items[0].Items[2];
                adminTab.IsVisible = false;
            }
        }
    }
}