using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Linq;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;
using NotionFlow.App.Views.Course;
using NotionFlow.App.Views.Auth;
using NotionFlow.App.Views.Teacher;
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
        public ObservableCollection<ActivityModel> Activities { get; } = new();

        public ICommand ShowOptionsCommand { get; }
        public ICommand ViewStudentProfileCommand { get; }

        public CourseViewModel(string courseId, string courseName, string role)
        {
            _courseId = courseId;
            CourseName = courseName;
            // accept both "Teacher" and "Profesor" so navigation from both flows works
            IsTeacher = role == "Teacher" || role == "Profesor" || role == "Professor";

            ShowOptionsCommand = new Command(async () =>
            {
                var option = await Shell.Current.DisplayActionSheet(
                    "¿Qué deseas agregar?", "Cancelar", null,
                    "Crear Evaluación", "Publicar Contenido", "Gestionar Actividades");

                if (option == "Crear Evaluación")
                    await Shell.Current.Navigation.PushAsync(new CreateEvaluationPage(this));
                else if (option == "Publicar Contenido")
                    await Shell.Current.Navigation.PushAsync(new PublishContentPage(this));
                else if (option == "Gestionar Actividades")
                    await Shell.Current.Navigation.PushAsync(
                        new ActivitiesPage(_api, int.Parse(_courseId), CourseName));
            });

            ViewStudentProfileCommand = new Command<StudentItem>(async (student) =>
            {
                if (student == null) return;
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
                foreach (var e in evaluations) Evaluations.Add(e);

                var contents = await _api.GetContentsAsync(int.Parse(_courseId));
                Contents.Clear();
                foreach (var c in contents) Contents.Add(c);

                var activities = await _api.GetActivitiesAsync(int.Parse(_courseId));
                Activities.Clear();
                foreach (var a in activities) Activities.Add(a);

                if (IsTeacher)
                {
                    var courses = await _api.GetCoursesByProfessorAsync(
                        AuthService.CurrentUser?.Id ?? string.Empty);
                    var course = courses.FirstOrDefault(c => c.Id.ToString() == _courseId);
                    if (course != null)
                    {
                        Students.Clear();
                        foreach (var s in course.Students) Students.Add(s);
                    }
                }
            }
            catch (Exception ex)
            {
                global::NotionFlow.App.CrashLog.Write("CourseViewModel.LoadDataAsync", ex);
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        public async Task CreateEvaluationAsync(string title, string description, double percentage)
        {
            await _api.CreateEvaluationAsync(int.Parse(_courseId), title, description, percentage);
            await LoadDataAsync();
        }

        public async Task PublishContentAsync(string title, string description, string type, string url)
        {
            await _api.PublishContentAsync(int.Parse(_courseId), title, description, type, url);
            await LoadDataAsync();
        }
    }
}
