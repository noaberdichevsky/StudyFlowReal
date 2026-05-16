using StudyFlow.ViewModels;

namespace StudyFlow.Views;

public partial class AddAssignmentView : ContentPage
{
    public AddAssignmentView(AddAssignmentViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}