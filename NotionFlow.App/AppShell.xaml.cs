using NotionFlow.App.Views.Auth;
using NotionFlow.App.Views.Admin;
using NotionFlow.App.Views.Course;

namespace NotionFlow.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("register", typeof(RegisterPage));
        Routing.RegisterRoute("curso", typeof(CoursePage));
        Routing.RegisterRoute("crearCurso", typeof(CreateCoursePage));
    }
}