using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;
using NotionFlow.App.Views.Course;

namespace NotionFlow.App.ViewModels.Student
{
    public class StudentViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private readonly string _studentId;

        public ObservableCollection<CourseResponse> Courses { get; } = new();
        public ICommand GoToCourseCommand { get; }
        public ICommand ViewCourseDetailsCommand { get; }
        public ICommand LogoutCommand { get; }

        public StudentViewModel(ApiService apiService, string studentId)
        {
            _api = apiService;
            _studentId = studentId;

            GoToCourseCommand = new Command<CourseResponse>(async (course) =>
            {
                if (course == null) return;
                var page = new CoursePage();
                page.BindingContext = new ViewModels.Course.CourseViewModel(
                    course.Id.ToString(), course.Name, "Student");
                await Shell.Current.Navigation.PushAsync(page);
            });

            ViewCourseDetailsCommand = new Command<CourseResponse>(async (course) =>
            {
                if (course == null) return;
                await Shell.Current.Navigation.PushAsync(new CourseDetailsPage(course, _api));
            });

            LogoutCommand = new Command(async () =>
            {
                await AuthService.LogoutAsync();
                await Shell.Current.GoToAsync("//login");
            });

            _ = LoadCoursesAsync();
        }

        private async Task LoadCoursesAsync()
        {
            try
            {
                var list = await _api.GetCoursesByStudentAsync(_studentId);
                Courses.Clear();
                foreach (var c in list) Courses.Add(c);
            }
            catch (Exception ex)
            {
                global::NotionFlow.App.CrashLog.Write("StudentViewModel.LoadCoursesAsync", ex);
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
