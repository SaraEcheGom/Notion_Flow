using NotionFlow.App.ViewModels.Admin;

namespace NotionFlow.App.Views.Admin;

public partial class CreateTeacherPage : ContentPage
{
    public CreateTeacherPage(AdminViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
