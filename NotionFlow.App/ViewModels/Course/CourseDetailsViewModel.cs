using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Diagnostics;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;

namespace NotionFlow.App.ViewModels.Course
{
    /// <summary>
    /// ViewModel para ver detalles de un curso.
    /// Utilizado por Admin, Teacher y Student para ver información del curso y estudiantes asignados.
    /// Solo Admin puede remover estudiantes.
    /// </summary>
    public class CourseDetailsViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private readonly CourseResponse _course;

        public int CourseId => _course.Id;
        public string CourseName => _course.Name;
        public string Subject => _course.Subject;
        public string TeacherName => _course.TeacherName;

        // Propiedad para controlar si el botón Remove debe estar visible
        public bool CanRemoveStudents => AuthService.CurrentUser?.Role == "Admin";

        public ObservableCollection<StudentItem> Students { get; } = new();

        public ICommand RemoveStudentCommand { get; }
        public ICommand BackCommand { get; }

        public CourseDetailsViewModel(ApiService api, CourseResponse course)
        {
            _api = api;
            _course = course;

            RemoveStudentCommand = new Command<StudentItem>(async (student) =>
            {
                if (student == null) return;

                // Validar que es Admin
                if (AuthService.CurrentUser?.Role != "Admin")
                {
                    Debug.WriteLine($"❌ [CourseDetailsViewModel] User role is '{AuthService.CurrentUser?.Role}', only Admin can remove students");
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
                    Debug.WriteLine($"\n🗑️ [CourseDetailsViewModel] RemoveStudentCommand called");
                    Debug.WriteLine($"   Course: {CourseName} (ID: {CourseId})");
                    Debug.WriteLine($"   Student: {student.Name} (ID: {student.Id})");
                    Debug.WriteLine($"   Authorized by: {AuthService.CurrentUser?.Name} (Role: {AuthService.CurrentUser?.Role})");

                    await _api.RemoveStudentAsync(CourseId, student.Id);

                    Debug.WriteLine($"✓ [CourseDetailsViewModel] Student removed successfully from database");

                    // Remover de la colección local
                    Students.Remove(student);
                    Debug.WriteLine($"✓ [CourseDetailsViewModel] Student removed from UI collection");

                    await Shell.Current.DisplayAlert("Success",
                        $"{student.Name} has been removed from {CourseName}", "OK");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ [CourseDetailsViewModel] Error: {ex.GetType().Name}");
                    Debug.WriteLine($"   Message: {ex.Message}");
                    Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
                    await Shell.Current.DisplayAlert("Error", 
                        $"Failed to remove student: {ex.Message}", "OK");
                }
            });

            BackCommand = new Command(async () =>
            {
                await Shell.Current.Navigation.PopAsync();
            });

            LoadStudents();
        }

        private void LoadStudents()
        {
            Debug.WriteLine($"📚 [CourseDetailsViewModel] Loading students for course {CourseName}");
            Debug.WriteLine($"   Total students: {_course.Students.Count}");
            Debug.WriteLine($"   Can remove: {CanRemoveStudents}");

            Students.Clear();
            foreach (var student in _course.Students)
            {
                Students.Add(student);
                Debug.WriteLine($"  ✓ {student.Name} ({student.Email})");
            }
        }
    }
}
