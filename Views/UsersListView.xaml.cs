using StudyFlow.ViewModels;

namespace StudyFlow.Views
{
    public partial class UsersListView : ContentPage
    {
        // Constructor receives UsersListViewModel via Dependency Injection
        public UsersListView(UsersListViewModel vm)
        {
            InitializeComponent();

            // Connect the ViewModel to this View
            BindingContext = vm;
        }

        // Called every time this page appears
        protected override void OnAppearing()
        {
            base.OnAppearing();
            (BindingContext as UsersListViewModel)!.OnAppearing();
        }

        // Called every time this page disappears
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            (BindingContext as UsersListViewModel)!.OnDisappearing();
        }
    }
}