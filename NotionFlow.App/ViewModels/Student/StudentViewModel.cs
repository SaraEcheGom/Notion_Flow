using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;

namespace NotionFlow.App.ViewModels.Student
{
    public class StudentViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private readonly string _studentId;

        public ObservableCollection<CourseResponse> Courses { get; } = new();
        public ICommand GoToCourseCommand { get; }
        public ICommand LogoutCommand { get; }

        public StudentViewModel(ApiService apiService, string studentId)
        {
            _api = apiService;
            _studentId = studentId;
            GoToCourseCommand = new Command<CourseResponse>(async (course) => await GoToCourseAsync(course));
            LogoutCommand = new Command(async () => await LogoutAsync());
            _ = LoadCoursesAsync();
        }

        private async Task LoadCoursesAsync()
        {
            try
            {
                var coursesList = await _api.GetCoursesByStudentAsync(_studentId);
                Courses.Clear();
                foreach (var course in coursesList) Courses.Add(course);
            }
            catch (Exception exception)
            {
                await Shell.Current.DisplayAlert("Error", exception.Message, "OK");
            }
        }

        private async Task GoToCourseAsync(CourseResponse course)
        {
            await Shell.Current.GoToAsync(
                $"course?courseId={course.Id}&courseName={course.Name}&role=Student");
        }

        private async Task LogoutAsync()
        {
            await AuthService.LogoutAsync();
            await Shell.Current.GoToAsync("//login");
        }
    }
}
