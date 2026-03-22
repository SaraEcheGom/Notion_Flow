using NotionFlow.App.ViewModels.Admin;

namespace NotionFlow.App.Views.Admin;

public partial class CreateProfessorPage : ContentPage
{
    public CreateProfessorPage(AdminViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
