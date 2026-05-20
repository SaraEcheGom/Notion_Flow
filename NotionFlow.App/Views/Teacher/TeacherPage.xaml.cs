using NotionFlow.App.ViewModels.Teacher;

namespace NotionFlow.App.Views.Teacher;

public partial class TeacherPage : ContentPage
{
    public TeacherPage(TeacherViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TeacherViewModel vm)
            vm.LoadCoursesCommand.Execute(null);
    }
}
