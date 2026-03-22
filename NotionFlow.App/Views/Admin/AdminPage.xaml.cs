using NotionFlow.App.ViewModels.Admin;

namespace NotionFlow.App.Views.Admin;

public partial class AdminPage : ContentPage
{
    public AdminPage()
    {
        InitializeComponent();
        BindingContext = new AdminViewModel();
    }
}
