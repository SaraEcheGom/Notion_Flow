using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;

namespace NotionFlow.App.ViewModels.Professor
{
    public class ProfessorViewModel : BaseViewModel
    {
        private readonly ApiService _api = new();
        private readonly string _professorId;

        public ObservableCollection<CourseResponse> Courses { get; } = new();

        public ICommand LoadCoursesCommand { get; }
        public ICommand GoToCourseCommand { get; }
        public ICommand LogoutCommand { get; }

        public ProfessorViewModel(string professorId)
        {
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
                var coursesList = await _api.GetCursosProfesorAsync(_professorId);
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
                $"course?courseId={course.Id}&courseName={course.Name}&role=Teacher");
        }

        private async System.Threading.Tasks.Task LogoutAsync()
        {
            await new AuthService().LogoutAsync();
            await Shell.Current.GoToAsync("//login");
        }
    }
}
