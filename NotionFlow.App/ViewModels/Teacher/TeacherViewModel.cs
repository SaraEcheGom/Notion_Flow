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
            GoToCourseCommand = new Command<CourseResponse>(async (course) => await GoToCourseAsync(course));
            ViewCourseDetailsCommand = new Command<CourseResponse>(async (course) =>
            {
                if (course == null) return;
                await Shell.Current.Navigation.PushAsync(
                    new CourseDetailsPage(course, _api));
            });
            LogoutCommand = new Command(async () => await LogoutAsync());
            _ = LoadCoursesAsync();
        }

        private async Task LoadCoursesAsync()
        {
            try
            {
                Debug.WriteLine("📚 [TeacherViewModel] Starting LoadCoursesAsync");
                Debug.WriteLine($"🔍 [TeacherViewModel] Calling GetCoursesByProfessorAsync('{_teacherId}')");
                var coursesList = await _api.GetCoursesByProfessorAsync(_teacherId);
                Debug.WriteLine($"✓ [TeacherViewModel] Got {coursesList.Count} courses");

                Courses.Clear();
                foreach (var course in coursesList) Courses.Add(course);
                Debug.WriteLine("✓ [TeacherViewModel] LoadCoursesAsync completed successfully");
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"✗ [TeacherViewModel] Error in LoadCoursesAsync: {exception.GetType().Name}");
                Debug.WriteLine($"✗ [TeacherViewModel] Message: {exception.Message}");
                await Shell.Current.DisplayAlert("Error", exception.Message, "OK");
            }
        }

        private async Task GoToCourseAsync(CourseResponse course)
        {
            await Shell.Current.GoToAsync(
                $"course?courseId={course.Id}&courseName={course.Name}&role=Teacher");
        }

        private async Task LogoutAsync()
        {
            await AuthService.LogoutAsync();
            await Shell.Current.GoToAsync("//login");
        }
    }
}
