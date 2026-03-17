using NotionFlow.App.ViewModels;

namespace NotionFlow.App.Views;

public partial class AdminPage : ContentPage
{
    public AdminPage()
    {
        InitializeComponent();
        BindingContext = new AdminViewModel();
    }
}