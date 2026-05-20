using NotionFlow.App.Services;
using NotionFlow.App.ViewModels.Student;

namespace NotionFlow.App.Views.Student;

public partial class StudentPage : ContentPage
{
    public StudentPage(ApiService apiService)
    {
        InitializeComponent();
        BindingContext = new StudentViewModel(apiService);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is StudentViewModel vm)
            _ = vm.RefreshAsync();
    }
}
