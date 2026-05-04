using NotionFlow.App.ViewModels.Admin;

namespace NotionFlow.App.Views.Admin;

public partial class CreateCoursePage : ContentPage
{
    public CreateCoursePage(AdminViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
