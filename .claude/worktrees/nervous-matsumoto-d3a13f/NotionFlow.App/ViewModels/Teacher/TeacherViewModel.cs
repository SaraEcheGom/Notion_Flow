using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Diagnostics;
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

        public TeacherViewModel(ApiService apiService, string teacherId)
        {
            Debug.WriteLine($"🎓 [TeacherViewModel] Constructor called with teacherId: {teacherId}");
            _api = apiService;
            _teacherId = teacherId;

            LoadCoursesCommand = new Command(async () => await LoadCoursesAsync());

            GoToCourseCommand = new Command<CourseResponse>(async (course) =>
            {
                if (course == null) return;
                var page = new CoursePage();
                page.BindingContext = new ViewModels.Course.CourseViewModel(
                    course.Id.ToString(), course.Name, "Teacher");
                await Shell.Current.Navigation.PushAsync(page);
            });

            ViewCourseDetailsCommand = new Command<CourseResponse>(async (course) =>
            {
                if (course == null) return;
                await Shell.Current.Navigation.PushAsync(new CourseDetailsPage(course, _api));
            });

            LogoutCommand = new Command(async () =>
            {
                await AuthService.LogoutAsync();
                await Shell.Current.GoToAsync("//login");
            });

            // Removed _ = LoadCoursesAsync(); to prevent duplicate/premature initialization before Binding Context is fully attached.
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

                if (list == null || list.Count == 0)
                {
                    Debug.WriteLine($"⚠️ [TeacherViewModel] No courses returned. Returning early.");
                    MainThread.BeginInvokeOnMainThread(() => IsBusy = false);
                    return;
                }

                try
                {
                    var courseNames = string.Join(", ", list.Select(c => c?.Name ?? "Unknown"));
                    Debug.WriteLine($"📋 [TeacherViewModel] Course names: {courseNames}");
                }
                catch (Exception nameEx)
                {
                    Debug.WriteLine($"⚠️ [TeacherViewModel] Error while formatting course names: {nameEx.Message}");
                }

                // Need to dispatch to main thread for UI updates in .NET MAUI CollectionView
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        Debug.WriteLine($"🔄 [TeacherViewModel] Updating UI with {list.Count} courses");
                        Courses.Clear();
                        foreach (var c in list)
                        {
                            if (c != null)
                            {
                                Debug.WriteLine($"➕ [TeacherViewModel] Adding course: {c.Id} - {c.Name}");
                                Courses.Add(c);
                            }
                        }
                        Debug.WriteLine($"✅ [TeacherViewModel] UI updated. Courses count: {Courses.Count}");
                    }
                    catch (Exception uiEx)
                    {
                        Debug.WriteLine($"❌ [TeacherViewModel] Error during UI update: {uiEx.GetType().Name}: {uiEx.Message}");
                        Debug.WriteLine($"❌ [TeacherViewModel] StackTrace: {uiEx.StackTrace}");
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✗ [TeacherViewModel] {ex.GetType().Name}: {ex.Message}");
                Debug.WriteLine($"✗ [TeacherViewModel] StackTrace: {ex.StackTrace}");
                global::NotionFlow.App.CrashLog.Write("TeacherViewModel.LoadCoursesAsync", ex);

                MainThread.BeginInvokeOnMainThread(async () => 
                {
                    await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
                    IsBusy = false;
                });
            }
        }
    }
}
