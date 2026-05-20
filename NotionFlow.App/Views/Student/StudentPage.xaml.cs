using NotionFlow.App.ViewModels.Student;

namespace NotionFlow.App.Views.Student;

public partial class StudentPage : ContentPage
{
    public StudentPage(StudentViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is StudentViewModel vm)
            _ = vm.RefreshAsync();
    }
}
