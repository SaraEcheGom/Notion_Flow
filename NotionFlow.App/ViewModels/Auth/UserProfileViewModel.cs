using System.Collections.ObjectModel;
using NotionFlow.App.Constants;
using NotionFlow.App.Models;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;

namespace NotionFlow.App.ViewModels.Auth
{
    public class UserProfileViewModel : BaseViewModel
    {
        private readonly ApiService _api;

        public string Name { get; }
        public string Email { get; }
        public string Role { get; }
        public string Initial => string.IsNullOrEmpty(Name) ? "?" :
            Name[0].ToString().ToUpper();

        public ObservableCollection<CourseResponse> Courses { get; } = new();

        public UserProfileViewModel(AuthResponse user, ApiService apiService)
        {
            _api = apiService ?? throw new ArgumentNullException(nameof(apiService));
            Name = user.Name;
            Email = user.Email;
            Role = user.Role;
            _ = LoadCoursesAsync(user);
        }

        private async Task LoadCoursesAsync(AuthResponse user)
        {
            var isProfessor = user.Role == Roles.Professor;
            var fetchFunc = isProfessor
                ? () => _api.GetCoursesByProfessorAsync(user.Id)
                : (Func<Task<List<CourseResponse>>>)(() => _api.GetCoursesByStudentAsync(user.Id));

            await ExecuteLoadAsync(fetchFunc, Courses, "UserProfileViewModel.LoadCoursesAsync");
        }
    }
}
