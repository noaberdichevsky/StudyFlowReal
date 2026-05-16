using StudyFlow.ViewModels;

namespace StudyFlow.Views;

public partial class UserProfileView : ContentPage
{
    public UserProfileView(UserProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}