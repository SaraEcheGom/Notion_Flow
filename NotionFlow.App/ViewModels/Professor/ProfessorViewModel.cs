using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Diagnostics;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;

namespace NotionFlow.App.ViewModels.Professor
{
    public class ProfessorViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private readonly string _professorId;

        public ObservableCollection<CourseResponse> Courses { get; } = new();

        public ICommand LoadCoursesCommand { get; }
        public ICommand GoToCourseCommand { get; }
        public ICommand LogoutCommand { get; }

        public ProfessorViewModel(ApiService apiService, string professorId)
        {
            _api = apiService;
            _professorId = professorId;
            LoadCoursesCommand = new Command(async () => await LoadCoursesAsync());
            GoToCourseCommand = new Command<CourseResponse>(async (course) => await GoToCourseAsync(course));
            LogoutCommand = new Command(async () => await LogoutAsync());
            _ = LoadCoursesAsync();
        }

        private async Task LoadCoursesAsync()
        {
            try
            {
                Debug.WriteLine("📚 [ProfessorViewModel] Starting LoadCoursesAsync");
                Debug.WriteLine($"🔍 [ProfessorViewModel] Calling GetCoursesByProfessorAsync('{_professorId}')");
                var coursesList = await _api.GetCoursesByProfessorAsync(_professorId);
                Debug.WriteLine($"✓ [ProfessorViewModel] Got {coursesList.Count} courses");

                Courses.Clear();
                foreach (var course in coursesList) Courses.Add(course);
                Debug.WriteLine("✓ [ProfessorViewModel] LoadCoursesAsync completed successfully");
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"✗ [ProfessorViewModel] Error in LoadCoursesAsync: {exception.GetType().Name}");
                Debug.WriteLine($"✗ [ProfessorViewModel] Message: {exception.Message}");
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
