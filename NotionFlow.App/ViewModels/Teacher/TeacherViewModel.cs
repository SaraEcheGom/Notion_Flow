using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;
using NotionFlow.App.Views.Course;

namespace NotionFlow.App.ViewModels.Teacher
{
    public class TeacherViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private readonly string _teacherId;

        public ObservableCollection<CourseResponse> Courses { get; } = new();

        public ICommand LoadCoursesCommand { get; }
        public ICommand GoToCourseCommand { get; }
        public ICommand ViewCourseDetailsCommand { get; }
        public ICommand LogoutCommand { get; }

        public TeacherViewModel(ApiService apiService)
        {
            _api = apiService;
            _teacherId = AuthService.CurrentUser?.Id ?? string.Empty;

            Debug.WriteLine($"🎓 [TeacherViewModel] teacherId: {_teacherId}");

            LoadCoursesCommand = new Command(async () => await LoadCoursesAsync());

            GoToCourseCommand = new Command<CourseResponse>(async course =>
            {
                if (course == null) return;
                var page = new CoursePage();
                page.BindingContext = new ViewModels.Course.CourseViewModel(
                    course.Id.ToString(), course.Name, "Teacher");
                await Shell.Current.Navigation.PushAsync(page);
            });

            ViewCourseDetailsCommand = new Command<CourseResponse>(async course =>
            {
                if (course == null) return;
                await Shell.Current.Navigation.PushAsync(new CourseDetailsPage(course, _api));
            });

            LogoutCommand = new Command(async () => await LogoutAsync());
        }

        private async Task LoadCoursesAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                Debug.WriteLine($"📚 [TeacherViewModel] LoadCoursesAsync — teacherId={_teacherId}");
                var list = await _api.GetCoursesByProfessorAsync(_teacherId);
                Debug.WriteLine($"✓ [TeacherViewModel] Got {list?.Count ?? 0} courses");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Courses.Clear();
                    foreach (var c in list ?? [])
                        if (c != null) Courses.Add(c);

                    Debug.WriteLine($"✅ [TeacherViewModel] Courses en UI: {Courses.Count}");
                    IsBusy = false;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✗ [TeacherViewModel] {ex.GetType().Name}: {ex.Message}");
                CrashLog.Write("TeacherViewModel.LoadCoursesAsync", ex);

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
                    IsBusy = false;
                });
            }
        }

        private async Task LogoutAsync()
        {
            await AuthService.LogoutAsync();

            if (Application.Current?.MainPage is AppShell shell)
                await shell.LogoutAsync();
            else
                await Shell.Current.GoToAsync("//login");
        }
    }
}