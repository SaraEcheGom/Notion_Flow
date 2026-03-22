using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NotionFlow.App.Models;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;

namespace NotionFlow.App.ViewModels.Auth
{
    public class UserProfileViewModel : BaseViewModel
    {
        private readonly ApiService _api = new();

        public string Name { get; }
        public string Email { get; }
        public string Role { get; }
        public string Initial => string.IsNullOrEmpty(Name) ? "?" :
            Name[0].ToString().ToUpper();

        public ObservableCollection<CourseResponse> Courses { get; } = new();

        public UserProfileViewModel(AuthResponse user)
        {
            Name = user.Name;
            Email = user.Email;
            Role = user.Role;
            _ = LoadCoursesAsync(user);
        }

        private async System.Threading.Tasks.Task LoadCoursesAsync(AuthResponse user)
        {
            try
            {
                List<CourseResponse> coursesList;

                if (user.Role == "Profesor")
                    coursesList = await _api.GetCursosProfesorAsync(user.Id);
                else
                    coursesList = await _api.GetCursosEstudianteAsync(user.Id);

                Courses.Clear();
                foreach (var course in coursesList) Courses.Add(course);
            }
            catch { }
        }
    }
}
