using StudyFlow.ViewModels;

namespace StudyFlow.Views
{
    public partial class MainPageView : ContentPage
    {
        // Constructor receives MainPageViewModel via Dependency Injection
        public MainPageView(MainPageViewModel vm)
        {
            InitializeComponent();

            // Connect the ViewModel to this View
            BindingContext = vm;
        }

        // Called every time this page appears on screen
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Tell the ViewModel the page appeared
            // so it refreshes the assignments list every time
            if (BindingContext is MainPageViewModel vm)
                await vm.OnAppearing();
        }
    }
}