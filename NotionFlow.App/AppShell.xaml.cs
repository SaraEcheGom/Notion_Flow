using NotionFlow.App.Views;

namespace NotionFlow.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("register", typeof(RegisterPage));
        Routing.RegisterRoute("curso", typeof(CursoPage));
        Routing.RegisterRoute("crearCurso", typeof(CrearCursoPage));
    }
}