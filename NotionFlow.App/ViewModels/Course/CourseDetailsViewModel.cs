using System.Collections.ObjectModel;
using System.Windows.Input;
using NotionFlow.App.Constants;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;

namespace NotionFlow.App.ViewModels.Course
{
    public class CourseDetailsViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;
        private readonly CourseResponse _course;

        public int CourseId => _course.Id;
        public string CourseName => _course.Name;
        public string Subject => _course.Subject;
        public string TeacherName => _course.TeacherName;

        public bool CanRemoveStudents => _auth.CurrentUser?.Role == Roles.Admin;

        public ObservableCollection<StudentItem> Students { get; } = new();

        public ICommand RemoveStudentCommand { get; }
        public ICommand BackCommand { get; }

        public CourseDetailsViewModel(ApiService api, AuthService auth, CourseResponse course)
        {
            _api = api;
            _auth = auth;
            _course = course;

            RemoveStudentCommand = new Command<StudentItem>(async (student) =>
            {
                if (student == null) return;

                if (_auth.CurrentUser?.Role != Roles.Admin)
                {
                    await Shell.Current.DisplayAlert("Error", "Only administrators can remove students from courses.", "OK");
                    return;
                }

                var confirm = await Shell.Current.DisplayAlert(
                    "Confirm Removal",
                    $"Remove {student.Name} from {CourseName}?\n\nThis action cannot be undone.",
                    "Remove", "Cancel");

                if (!confirm) return;

                try
                {
                    await _api.RemoveStudentAsync(CourseId, student.Id);
                    Students.Remove(student);
                    await Shell.Current.DisplayAlert("Success",
                        $"{student.Name} has been removed from {CourseName}", "OK");
                }
                catch (Exception ex)
                {
                    CrashLog.Write("CourseDetailsViewModel.RemoveStudent", ex);
                    await Shell.Current.DisplayAlert("Error",
                        $"Failed to remove student: {ex.Message}", "OK");
                }
            });

            BackCommand = new Command(async () => await Shell.Current.Navigation.PopAsync());

            LoadStudents();
        }

        private void LoadStudents()
        {
            Students.Clear();
            foreach (var student in _course.Students)
                Students.Add(student);
        }
    }
}
