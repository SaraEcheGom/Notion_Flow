using System.Collections.ObjectModel;
using System.Windows.Input;
using NotionFlow.App.Constants;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;
using NotionFlow.App.Views.Course;

namespace NotionFlow.App.ViewModels.Teacher
{
    public class TeacherViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;
        private readonly string _teacherId;

        public ObservableCollection<CourseResponse> Courses { get; } = new();

        public ICommand LoadCoursesCommand { get; }
        public ICommand GoToCourseCommand { get; }
        public ICommand ViewCourseDetailsCommand { get; }
        public ICommand LogoutCommand { get; }

        public TeacherViewModel(ApiService apiService, AuthService authService)
        {
            _api = apiService;
            _auth = authService;
            _teacherId = authService.CurrentUser?.Id ?? string.Empty;

            LoadCoursesCommand = new Command(async () => await LoadCoursesAsync());

            GoToCourseCommand = new Command<CourseResponse>(async course =>
            {
                if (course == null) return;
                var page = new CoursePage();
                page.BindingContext = new ViewModels.Course.CourseViewModel(
                    _api, _auth, course.Id.ToString(), course.Name, Roles.Professor);
                await Shell.Current.Navigation.PushAsync(page);
            });

            ViewCourseDetailsCommand = new Command<CourseResponse>(async course =>
            {
                if (course == null) return;
                await Shell.Current.Navigation.PushAsync(new CourseDetailsPage(course, _api, _auth));
            });

            LogoutCommand = new Command(async () => await LogoutAsync());
        }

        public Task LoadCoursesAsync() =>
            ExecuteLoadAsync(
                () => _api.GetCoursesByProfessorAsync(_teacherId),
                Courses,
                "TeacherViewModel.LoadCoursesAsync");

        private async Task LogoutAsync()
        {
            await _auth.LogoutAsync();

            if (Application.Current?.MainPage is AppShell shell)
                await shell.LogoutAsync();
            else
                await Shell.Current.GoToAsync(Routes.Login);
        }
    }
}
