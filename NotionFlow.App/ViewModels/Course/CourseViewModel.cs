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
        public bool IsStudent => !IsTeacher;

        public ObservableCollection<Evaluation> Evaluations { get; } = new();
        public ObservableCollection<Content> Contents { get; } = new();
        public ObservableCollection<StudentItem> Students { get; } = new();
        public ObservableCollection<ActivityModel> Activities { get; } = new();

        public ICommand ShowOptionsCommand { get; }
        public ICommand ViewStudentProfileCommand { get; }
        public ICommand EditActivityCommand { get; }
        public ICommand DeleteActivityCommand { get; }
        public ICommand TakeActivityCommand { get; }
        public ICommand ViewResultsCommand { get; }
        // HU#14: Ver progreso personal (estudiante)
        public ICommand ViewMyProgressCommand { get; }
        // HU#15: Ver reporte del curso (profesor)
        public ICommand ViewCourseReportCommand { get; }

        private NotionFlow.App.ViewModels.Teacher.ActivityViewModel? _actVm;

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
                    "Crear Evaluación", "Publicar Contenido", "Crear Actividad");

                if (option == "Crear Evaluación")
                    await Shell.Current.Navigation.PushAsync(new CreateEvaluationPage(this));
                else if (option == "Publicar Contenido")
                    await Shell.Current.Navigation.PushAsync(new PublishContentPage(this));
                else if (option == "Crear Actividad")
                {
                    var actVm = new NotionFlow.App.ViewModels.Teacher.ActivityViewModel(_api, int.Parse(_courseId), CourseName);
                    var createPage = new NotionFlow.App.Views.Teacher.CreateActivityPage(actVm);
                    createPage.ActivityCreated += async () => await LoadDataAsync();
                    await Shell.Current.Navigation.PushAsync(createPage);
                }
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

            EditActivityCommand = new Command<ActivityModel>(async (activity) =>
            {
                if (activity == null) return;
                _actVm ??= new NotionFlow.App.ViewModels.Teacher.ActivityViewModel(_api, int.Parse(_courseId), CourseName);
                // Sync current activities into actVm
                _actVm.Activities.Clear();
                foreach (var a in Activities) _actVm.Activities.Add(a);
                var editPage = new NotionFlow.App.Views.Teacher.EditActivityPage(_actVm, activity);
                editPage.ActivityUpdated += async () => await LoadDataAsync();
                await Shell.Current.Navigation.PushAsync(editPage);
            });

            DeleteActivityCommand = new Command<ActivityModel>(async (activity) =>
            {
                if (activity == null) return;
                var confirm = await Shell.Current.DisplayAlert("Confirmar",
                    $"¿Eliminar \"{activity.Title}\"?", "Eliminar", "Cancelar");
                if (!confirm) return;
                try
                {
                    await _api.DeleteActivityAsync(int.Parse(_courseId), activity.Id);
                    Activities.Remove(activity);
                    await Shell.Current.DisplayAlert("Éxito", "Actividad eliminada.", "OK");
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
                }
            });

            TakeActivityCommand = new Command<ActivityModel>(async (activity) =>
            {
                if (activity == null) return;
                await Shell.Current.Navigation.PushAsync(
                    new NotionFlow.App.Views.Student.TakeActivityPage(_api, activity));
            });

            ViewResultsCommand = new Command<ActivityModel>(async (activity) =>
            {
                if (activity == null) return;
                await Shell.Current.Navigation.PushAsync(
                    new NotionFlow.App.Views.Teacher.ActivityResultsPage(
                        _api, int.Parse(_courseId), activity.Id, activity.Title));
            });

            // HU#14: Ver progreso personal del estudiante
            ViewMyProgressCommand = new Command(async () =>
            {
                var studentId = AuthService.CurrentUser?.Id ?? string.Empty;
                if (string.IsNullOrEmpty(studentId)) return;
                await Shell.Current.Navigation.PushAsync(
                    new NotionFlow.App.Views.Student.StudentProgressPage(
                        _api, int.Parse(_courseId), studentId));
            });

            // HU#15: Ver reporte general del curso (profesor)
            ViewCourseReportCommand = new Command(async () =>
            {
                await Shell.Current.Navigation.PushAsync(
                    new NotionFlow.App.Views.Teacher.CourseReportPage(
                        _api, int.Parse(_courseId), CourseName));
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
