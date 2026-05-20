using NotionFlow.App.Constants;
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

        Routing.RegisterRoute(Routes.Register, typeof(RegisterPage));

        // Admin
        Routing.RegisterRoute(Routes.AdminCreateCourse, typeof(CreateCoursePage));
        Routing.RegisterRoute(Routes.AdminCreateTeacher, typeof(CreateTeacherPage));
        Routing.RegisterRoute(Routes.AdminCreateStudent, typeof(CreateStudentPage));
        Routing.RegisterRoute(Routes.AdminCourseDetail, typeof(CourseDetailsPage));

        // Profesor
        Routing.RegisterRoute(Routes.TeacherCourseDetail, typeof(CourseDetailsPage));
        Routing.RegisterRoute(Routes.TeacherCreateActivity, typeof(CreateActivityPage));
        Routing.RegisterRoute(Routes.TeacherEditActivity, typeof(EditActivityPage));
        Routing.RegisterRoute(Routes.TeacherAssignActivity, typeof(AssignActivityPage));
        Routing.RegisterRoute(Routes.TeacherPublishContent, typeof(PublishContentPage));
        Routing.RegisterRoute(Routes.TeacherCreateEval, typeof(CreateEvaluationPage));
        Routing.RegisterRoute(Routes.TeacherActivityResults, typeof(ActivityResultsPage));

        // Estudiante
        Routing.RegisterRoute(Routes.StudentCourseDetail, typeof(CourseDetailsPage));
        Routing.RegisterRoute(Routes.StudentTakeActivity, typeof(TakeActivityPage));
    }

    public async Task ShowRoleTabsAsync(string role)
    {
        LoginSection.IsVisible = false;
        AdminTabs.IsVisible = false;
        TeacherTabs.IsVisible = false;
        StudentTabs.IsVisible = false;

        switch (role.Trim())
        {
            case Roles.Admin:
                AdminTabs.IsVisible = true;
                await GoToAsync(Routes.AdminHome);
                break;

            case Roles.Professor:
                TeacherTabs.IsVisible = true;
                await GoToAsync(Routes.TeacherHome);
                break;

            case Roles.Student:
                StudentTabs.IsVisible = true;
                await GoToAsync(Routes.StudentHome);
                break;

            default:
                await DisplayAlert("Error", $"Rol '{role}' no reconocido.", "OK");
                LoginSection.IsVisible = true;
                break;
        }
    }

    public async Task LogoutAsync()
    {
        AdminTabs.IsVisible = false;
        TeacherTabs.IsVisible = false;
        StudentTabs.IsVisible = false;
        LoginSection.IsVisible = true;
        await GoToAsync(Routes.Login);
    }
}
