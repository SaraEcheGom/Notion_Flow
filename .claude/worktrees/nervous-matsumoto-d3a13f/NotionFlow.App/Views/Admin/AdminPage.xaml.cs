using NotionFlow.App.ViewModels.Admin;
using NotionFlow.App.Services;

namespace NotionFlow.App.Views.Admin;

public partial class AdminPage : ContentPage
{
    public AdminPage()
    {
        InitializeComponent();
        var apiService = new ApiService();
        BindingContext = new AdminViewModel(apiService);

        ConfigurePageHeader();
    }

    private void ConfigurePageHeader()
    {
        AdminHeader.Title = "Panel de Administrador";
        AdminHeader.Subtitle = "Gestiona cursos, profesores y estudiantes";
    }
}
