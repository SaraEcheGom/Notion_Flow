using NotionFlow.App.Views.Admin;
using NotionFlow.App.Views.Teacher;
using NotionFlow.App.Views.Student;
using NotionFlow.App.Views.Course;
using NotionFlow.App.Views.Auth;
using NotionFlow.App.Services;

namespace NotionFlow.App;

public partial class AppShell : Shell
{
    private readonly ApiService _apiService;

    public AppShell(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;

        // ── Rutas de detalle — accesibles desde cualquier rol ─────────────
        Routing.RegisterRoute("register", typeof(RegisterPage));

        // Admin
        Routing.RegisterRoute("admin/create-course", typeof(CreateCoursePage));
        Routing.RegisterRoute("admin/create-teacher", typeof(CreateTeacherPage));
        Routing.RegisterRoute("admin/create-student", typeof(CreateStudentPage));
        Routing.RegisterRoute("admin/course-detail", typeof(CourseDetailsPage));

        // Profesor
        Routing.RegisterRoute("teacher/course-detail", typeof(CourseDetailsPage));
        Routing.RegisterRoute("teacher/create-activity", typeof(CreateActivityPage));
        Routing.RegisterRoute("teacher/edit-activity", typeof(EditActivityPage));
        Routing.RegisterRoute("teacher/assign-activity", typeof(AssignActivityPage));
        Routing.RegisterRoute("teacher/publish-content", typeof(PublishContentPage));
        Routing.RegisterRoute("teacher/create-eval", typeof(CreateEvaluationPage));
        Routing.RegisterRoute("teacher/activity-results", typeof(ActivityResultsPage));

        // Estudiante
        Routing.RegisterRoute("student/course-detail", typeof(CourseDetailsPage));
        Routing.RegisterRoute("student/take-activity", typeof(TakeActivityPage));
    }

    /// <summary>
    /// Llamado desde LoginViewModel después de autenticar.
    /// Oculta el login, muestra los tabs del rol y navega al inicio.
    /// </summary>
    public async Task ShowRoleTabsAsync(string role)
    {
        // Ocultar todo
        LoginSection.IsVisible = false;
        AdminTabs.IsVisible = false;
        TeacherTabs.IsVisible = false;
        StudentTabs.IsVisible = false;

        switch (role.Trim())
        {
            case "Admin":
                AdminTabs.IsVisible = true;
                await GoToAsync("//admin_home");
                break;

            case "Professor":
            case "Profesor":
                TeacherTabs.IsVisible = true;
                await GoToAsync("//teacher_home");
                break;

            case "Student":
            case "Estudiante":
                StudentTabs.IsVisible = true;
                await GoToAsync("//student_home");
                break;

            default:
                await DisplayAlert(
                    "Error",
                    $"Rol '{role}' no reconocido.",
                    "OK");
                LoginSection.IsVisible = true;
                break;
        }
    }

    /// <summary>
    /// Llamado desde logout — regresa al login y limpia los tabs.
    /// </summary>
    public async Task LogoutAsync()
    {
        AdminTabs.IsVisible = false;
        TeacherTabs.IsVisible = false;
        StudentTabs.IsVisible = false;
        LoginSection.IsVisible = true;
        await GoToAsync("//login");
    }
}