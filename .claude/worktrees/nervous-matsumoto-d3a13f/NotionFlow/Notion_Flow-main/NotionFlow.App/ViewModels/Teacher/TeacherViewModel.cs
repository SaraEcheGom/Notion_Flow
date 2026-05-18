using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Diagnostics;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;
using NotionFlow.App.Views.Course;

namespace NotionFlow.App.ViewModels.Teacher
{
    public class TeacherViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private readonly string _teacherId;

        public ObservableCollection<CourseResponse> Courses { get; } = new();

        public ICommand LoadCoursesCommand { get; }
        public ICommand GoToCourseCommand { get; }
        public ICommand ViewCourseDetailsCommand { get; }
        public ICommand LogoutCommand { get; }

        public TeacherViewModel(ApiService apiService, string teacherId)
        {
            _api = apiService;
            _teacherId = teacherId;

            LoadCoursesCommand = new Command(async () => await LoadCoursesAsync());

            GoToCourseCommand = new Command<CourseResponse>(async (course) =>
            {
                if (course == null) return;
                var page = new CoursePage();
                page.BindingContext = new ViewModels.Course.CourseViewModel(
                    course.Id.ToString(), course.Name, "Teacher");
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
                Debug.WriteLine($"📚 [TeacherViewModel] LoadCoursesAsync — teacherId={_teacherId}");
                var list = await _api.GetCoursesByProfessorAsync(_teacherId);
                Debug.WriteLine($"✓ [TeacherViewModel] Got {list.Count} courses");
                Courses.Clear();
                foreach (var c in list) Courses.Add(c);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✗ [TeacherViewModel] {ex.GetType().Name}: {ex.Message}");
                global::NotionFlow.App.CrashLog.Write("TeacherViewModel.LoadCoursesAsync", ex);
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
