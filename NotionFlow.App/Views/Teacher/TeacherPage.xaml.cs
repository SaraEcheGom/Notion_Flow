using NotionFlow.App.Services;
using NotionFlow.App.ViewModels.Teacher;

namespace NotionFlow.App.Views.Teacher;

public partial class TeacherPage : ContentPage
{
    public TeacherPage(ApiService apiService)
    {
        InitializeComponent();
        BindingContext = new TeacherViewModel(apiService);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TeacherViewModel vm)
            vm.LoadCoursesCommand.Execute(null);
    }
}
