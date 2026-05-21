using System.Collections.ObjectModel;
using System.Windows.Input;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;
using NotionFlow.App.Views.Course;
using NotionFlow.App.Views.Student;

namespace NotionFlow.App.ViewModels.Student
{
    public class StudentViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;
        private readonly string _studentId;

        public ObservableCollection<CourseResponse> Courses { get; } = new();

        public ICommand GoToCourseCommand { get; }
        public ICommand ViewCourseDetailsCommand { get; }
        public ICommand GoToProgressCommand { get; }
        public ICommand LogoutCommand { get; }

        public StudentViewModel(ApiService apiService, AuthService authService)
        {
            _api = apiService;
            _auth = authService;
            _studentId = authService.CurrentUser?.Id ?? string.Empty;

            GoToCourseCommand = new Command<CourseResponse>(async course =>
            {
                if (course == null) return;
                var page = new CoursePage();
                page.BindingContext = new ViewModels.Course.CourseViewModel(
                    _api, _auth, course.Id.ToString(), course.Name, "Student");
                await Shell.Current.Navigation.PushAsync(page);
            });

            ViewCourseDetailsCommand = new Command<CourseResponse>(async course =>
            {
                if (course == null) return;
                await Shell.Current.Navigation.PushAsync(
                    new CourseDetailsPage(course, _api, _auth));
            });

            // Navega a la página de progreso del estudiante para el curso seleccionado.
            // Se instancia directamente con new porque requiere parámetros de instancia
            // (courseId, studentId) que DI no puede resolver.
            GoToProgressCommand = new Command<CourseResponse>(async course =>
            {
                if (course == null) return;
                await Shell.Current.Navigation.PushAsync(
                    new StudentProgressPage(_api, course.Id, _studentId));
            });

            LogoutCommand = new Command(async () => await LogoutAsync());
        }

        public Task RefreshAsync() => LoadCoursesAsync();

        public Task LoadCoursesAsync() =>
            ExecuteLoadAsync(
                () => _api.GetCoursesByStudentAsync(_studentId),
                Courses,
                "StudentViewModel.LoadCoursesAsync");

        private async Task LogoutAsync()
        {
            await _auth.LogoutAsync();

            if (Application.Current?.MainPage is AppShell shell)
                await shell.LogoutAsync();
            else
                await Shell.Current.GoToAsync("//login");
        }
    }
}