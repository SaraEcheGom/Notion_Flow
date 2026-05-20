using NotionFlow.App.Services;
using NotionFlow.App.ViewModels.Teacher;

namespace NotionFlow.App.Views.Teacher;

public partial class TeacherDashboardPage : ContentPage
{
    public TeacherDashboardPage(ApiService api, AuthService auth)
    {
        InitializeComponent();
        BindingContext = new TeacherDashboardViewModel(api, auth);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TeacherDashboardViewModel vm)
            vm.LoadDashboardCommand.Execute(null);
    }
}