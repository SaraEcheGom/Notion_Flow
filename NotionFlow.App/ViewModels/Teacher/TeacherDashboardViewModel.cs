using System.Collections.ObjectModel;
using System.Windows.Input;
using NotionFlow.App.Constants;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;
using NotionFlow.App.Views.Course;

namespace NotionFlow.App.ViewModels.Teacher
{
    public class TeacherDashboardViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;
        private readonly string _teacherId;

        private int _totalCourses;
        private int _totalStudents;
        private int _pendingEvaluations;
        private int _activeActivities;

        public int TotalCourses
        {
            get => _totalCourses;
            set => SetProperty(ref _totalCourses, value);
        }

        public int TotalStudents
        {
            get => _totalStudents;
            set => SetProperty(ref _totalStudents, value);
        }

        public int PendingEvaluations
        {
            get => _pendingEvaluations;
            set => SetProperty(ref _pendingEvaluations, value);
        }

        public int ActiveActivities
        {
            get => _activeActivities;
            set => SetProperty(ref _activeActivities, value);
        }

        public ObservableCollection<CourseResponse> RecentCourses { get; } = new();

        public ICommand LoadDashboardCommand { get; }
        public ICommand ViewEvaluationsCommand { get; }
        public ICommand ViewActivitiesCommand { get; }

        public TeacherDashboardViewModel(ApiService apiService, AuthService authService)
        {
            _api = apiService;
            _auth = authService;
            _teacherId = authService.CurrentUser?.Id ?? string.Empty;

            LoadDashboardCommand = new Command(async () => await LoadDashboardAsync());

            ViewEvaluationsCommand = new Command(async () =>
            {
                await Shell.Current.DisplayAlert("Evaluaciones", 
                    "Accede a Mis Cursos y selecciona un curso para ver sus evaluaciones.", "OK");
            });

            ViewActivitiesCommand = new Command(async () =>
            {
                await Shell.Current.DisplayAlert("Actividades", 
                    "Accede a Mis Cursos y selecciona un curso para crear/editar actividades.", "OK");
            });
        }

        public async Task LoadDashboardAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                var courses = await _api.GetCoursesByProfessorAsync(_teacherId);

                if (courses == null || courses.Count == 0)
                {
                    TotalCourses = 0;
                    TotalStudents = 0;
                    PendingEvaluations = 0;
                    ActiveActivities = 0;
                    RecentCourses.Clear();
                    return;
                }

                TotalCourses = courses.Count;

                // Contar estudiantes únicos
                var uniqueStudents = courses
                    .SelectMany(c => c.Students ?? new List<StudentItem>())
                    .DistinctBy(s => s.Id)
                    .ToList();
                TotalStudents = uniqueStudents.Count;

                // Valores por defecto (no disponibles en CourseResponse)
                PendingEvaluations = 0;
                ActiveActivities = 0;

                // Mostrar primeros 5 cursos
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    RecentCourses.Clear();
                    foreach (var course in courses.Take(5))
                    {
                        RecentCourses.Add(course);
                    }
                });
            }
            catch (Exception ex)
            {
                CrashLog.Write("TeacherDashboardViewModel.LoadDashboardAsync", ex);
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Shell.Current.DisplayAlert("Error", $"No se pudo cargar el dashboard: {ex.Message}", "OK"));
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}