using System.Collections.ObjectModel;
using System.Windows.Input;
using NotionFlow.App.Constants;
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
        private readonly ApiService _api;
        private readonly AuthService _auth;
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
        public ICommand GoToProgressCommand { get; }

        private NotionFlow.App.ViewModels.Teacher.ActivityViewModel? _actVm;

        public CourseViewModel(ApiService api, AuthService auth, string courseId, string courseName, string role)
        {
            _api = api;
            _auth = auth;
            _courseId = courseId;
            CourseName = courseName;
            IsTeacher = role == Roles.Professor;

            ShowOptionsCommand = new Command(async () =>
            {
                var option = await Shell.Current.DisplayActionSheet(
                    "¿Qué deseas agregar?", "Cancelar", null,
                    "Crear Evaluación", "Publicar Contenido", "Crear Actividad",
                    "Generar cuestionario desde foto");

                if (option == "Crear Evaluación")
                    await Shell.Current.Navigation.PushAsync(new CreateEvaluationPage(this));
                else if (option == "Publicar Contenido")
                    await Shell.Current.Navigation.PushAsync(new PublishContentPage(this));
                else if (option == "Crear Actividad")
                {
                    var actVm = new NotionFlow.App.ViewModels.Teacher.ActivityViewModel(_api, int.Parse(_courseId), CourseName);
                    var createPage = new CreateActivityPage(actVm);
                    createPage.ActivityCreated += async () => await LoadDataAsync();
                    await Shell.Current.Navigation.PushAsync(createPage);
                }
                else if (option == "Generar cuestionario desde foto")
                {
                    var actVm = new NotionFlow.App.ViewModels.Teacher.ActivityViewModel(_api, int.Parse(_courseId), CourseName);
                    var generatePage = new GenerateQuizFromImagePage(_api, actVm);
                    await Shell.Current.Navigation.PushAsync(generatePage);
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
                    Role = Roles.Student
                };
                await Shell.Current.Navigation.PushAsync(
                    new UserProfilePage(new UserProfileViewModel(user, _api)));
            });

            EditActivityCommand = new Command<ActivityModel>(async (activity) =>
            {
                if (activity == null) return;
                _actVm ??= new NotionFlow.App.ViewModels.Teacher.ActivityViewModel(_api, int.Parse(_courseId), CourseName);
                _actVm.Activities.Clear();
                foreach (var a in Activities) _actVm.Activities.Add(a);
                var editPage = new EditActivityPage(_actVm, activity);
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
                    CrashLog.Write("CourseViewModel.DeleteActivity", ex);
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
                    new ActivityResultsPage(_api, int.Parse(_courseId), activity.Id, activity.Title));
            });

            GoToProgressCommand = new Command(async () =>
            {
                await Shell.Current.Navigation.PushAsync(
                    new NotionFlow.App.Views.Course.CourseProgressPage(_api, int.Parse(_courseId), CourseName));
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
                        _auth.CurrentUser?.Id ?? string.Empty);
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
                CrashLog.Write("CourseViewModel.LoadDataAsync", ex);
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        public async Task CreateEvaluationAsync(string title, string description, double percentage, DateTime date)
        {
            await _api.CreateEvaluationAsync(int.Parse(_courseId), title, description, percentage, date);
            await LoadDataAsync();
        }

        public async Task PublishContentAsync(string title, string description, string type, string url)
        {
            await _api.PublishContentAsync(int.Parse(_courseId), title, description, type, url);
            await LoadDataAsync();
        }
    }
}
