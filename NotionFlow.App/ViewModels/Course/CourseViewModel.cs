using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Linq;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;
using NotionFlow.App.Views.Course;
using NotionFlow.App.Views.Professor;
using NotionFlow.App.Views.Auth;
using NotionFlow.App.ViewModels.Auth;

namespace NotionFlow.App.ViewModels.Course
{
    public class CourseViewModel : BaseViewModel
    {
        private readonly ApiService _api = new();
        private readonly string _courseId;

        public string CourseName { get; }
        public bool IsTeacher { get; }

        public ObservableCollection<Evaluation> Evaluations { get; } = new();
        public ObservableCollection<Content> Contents { get; } = new();
        public ObservableCollection<StudentItem> Students { get; } = new();

        public ICommand ShowOptionsCommand { get; }
        public ICommand ViewStudentProfileCommand { get; }

        public CourseViewModel(string courseId, string courseName, string role)
        {
            _courseId = courseId;
            CourseName = courseName;
            IsTeacher = role == "Profesor";

            ShowOptionsCommand = new Command(async () =>
            {
                var option = await Shell.Current.DisplayActionSheet(
                    "What would you like to add?", "Cancel", null,
                    "Create Evaluation", "Publish Content");

                if (option == "Create Evaluation")
                    await Shell.Current.Navigation.PushAsync(
                        new CreateEvaluationPage(this));
                else if (option == "Publish Content")
                    await Shell.Current.Navigation.PushAsync(
                        new PublishContentPage(this));
            });

            ViewStudentProfileCommand = new Command<StudentItem>(async (student) =>
            {
                var user = new AuthResponse
                {
                    Id = student.Id,
                    Name = student.Name,
                    Email = student.Email,
                    Role = "Estudiante"
                };
                await Shell.Current.Navigation.PushAsync(
                    new UserProfilePage(new UserProfileViewModel(user)));
            });

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var evaluations = await _api.GetEvaluationsAsync(int.Parse(_courseId));
                Evaluations.Clear();
                foreach (var evaluation in evaluations) Evaluations.Add(evaluation);

                var contents = await _api.GetContentsAsync(int.Parse(_courseId));
                Contents.Clear();
                foreach (var content in contents) Contents.Add(content);

                if (IsTeacher)
                {
                    var courses = await _api.GetCoursesByProfessorAsync(
                        AuthService.CurrentUser?.Id ?? string.Empty);
                    var course = courses.FirstOrDefault(c => c.Id.ToString() == _courseId);
                    if (course != null)
                    {
                        Students.Clear();
                        foreach (var student in course.Students) Students.Add(student);
                    }
                }
            }
            catch (Exception exception)
            {
                await Shell.Current.DisplayAlertAsync("Error", exception.Message, "OK");
            }
        }

        public async Task CreateEvaluationAsync(string title,
            string description, double percentage)
        {
            await _api.CreateEvaluationAsync(int.Parse(_courseId),
                title, description, percentage);
            await LoadDataAsync();
        }

        public async Task PublishContentAsync(string title,
            string description, string type, string url)
        {
            await _api.PublishContentAsync(int.Parse(_courseId),
                title, description, type, url);
            await LoadDataAsync();
        }
    }
}
