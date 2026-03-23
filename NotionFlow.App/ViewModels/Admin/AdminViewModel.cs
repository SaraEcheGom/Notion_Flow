using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Diagnostics;
using NotionFlow.App.Services;
using NotionFlow.App.Views.Admin;
using NotionFlow.App.Views.Auth;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.ViewModels.Auth;

namespace NotionFlow.App.ViewModels.Admin
{
    public class AdminViewModel : BaseViewModel
    {
        private readonly ApiService _api;

        public ObservableCollection<AuthResponse> Teachers { get; } = new();
        public ObservableCollection<AuthResponse> Students { get; } = new();
        public ObservableCollection<CourseResponse> Courses { get; } = new();

        private AuthResponse? _selectedTeacher;
        private AuthResponse? _selectedStudent;
        private CourseResponse? _selectedCourse;
        private string _courseName = string.Empty;
        private string _courseSubject = string.Empty;
        private string _teacherName = string.Empty;
        private string _teacherEmail = string.Empty;
        private string _teacherPassword = string.Empty;

        public AuthResponse? SelectedTeacher
        {
            get => _selectedTeacher;
            set { _selectedTeacher = value; OnPropertyChanged(); }
        }

        public AuthResponse? SelectedStudent
        {
            get => _selectedStudent;
            set { _selectedStudent = value; OnPropertyChanged(); }
        }

        public CourseResponse? SelectedCourse
        {
            get => _selectedCourse;
            set { _selectedCourse = value; OnPropertyChanged(); }
        }

        public string CourseName
        {
            get => _courseName;
            set { _courseName = value; OnPropertyChanged(); }
        }

        public string CourseSubject
        {
            get => _courseSubject;
            set { _courseSubject = value; OnPropertyChanged(); }
        }

        public string TeacherName
        {
            get => _teacherName;
            set { _teacherName = value; OnPropertyChanged(); }
        }

        public string TeacherEmail
        {
            get => _teacherEmail;
            set { _teacherEmail = value; OnPropertyChanged(); }
        }

        public string TeacherPassword
        {
            get => _teacherPassword;
            set { _teacherPassword = value; OnPropertyChanged(); }
        }

        public ICommand LoadDataCommand { get; }
        public ICommand CreateCourseCommand { get; }
        public ICommand AssignStudentCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand GoToCreateCourseCommand { get; }
        public ICommand GoToCreateTeacherCommand { get; }
        public ICommand CreateTeacherCommand { get; }
        public ICommand ViewProfileCommand { get; }

        public AdminViewModel(ApiService apiService)
        {
            _api = apiService;
            LoadDataCommand = new Command(async () => await LoadDataAsync());
            CreateCourseCommand = new Command(async () => await CreateCourseAsync());
            AssignStudentCommand = new Command(async () => await AssignStudentAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());
            CreateTeacherCommand = new Command(async () => await CreateTeacherAsync());

            GoToCreateCourseCommand = new Command(async () =>
                await Shell.Current.Navigation.PushAsync(new CreateCoursePage(this)));

            GoToCreateTeacherCommand = new Command(async () =>
                await Shell.Current.Navigation.PushAsync(new CreateProfessorPage(this)));

            ViewProfileCommand = new Command<AuthResponse>(async (u) =>
                await Shell.Current.Navigation.PushAsync(
                    new UserProfilePage(new UserProfileViewModel(u))));

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                Debug.WriteLine("📊 [AdminViewModel] Starting LoadDataAsync");

                Debug.WriteLine("🔍 [AdminViewModel] Calling GetUsersByRoleAsync('Professor')");
                var teachers = await _api.GetUsersByRoleAsync("Professor");
                Debug.WriteLine($"✓ [AdminViewModel] Got {teachers.Count} teachers");

                Debug.WriteLine("🔍 [AdminViewModel] Calling GetUsersByRoleAsync('Student')");
                var students = await _api.GetUsersByRoleAsync("Student");
                Debug.WriteLine($"✓ [AdminViewModel] Got {students.Count} students");

                Debug.WriteLine("🔍 [AdminViewModel] Calling GetAllCoursesAsync()");
                var courses = await _api.GetAllCoursesAsync();
                Debug.WriteLine($"✓ [AdminViewModel] Got {courses.Count} courses");

                Teachers.Clear();
                foreach (var teacher in teachers) Teachers.Add(teacher);

                Students.Clear();
                foreach (var student in students) Students.Add(student);

                Courses.Clear();
                foreach (var course in courses) Courses.Add(course);

                Debug.WriteLine("✓ [AdminViewModel] LoadDataAsync completed successfully");
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"✗ [AdminViewModel] Error in LoadDataAsync: {exception.GetType().Name}");
                Debug.WriteLine($"✗ [AdminViewModel] Message: {exception.Message}");
                Debug.WriteLine($"✗ [AdminViewModel] StackTrace: {exception.StackTrace}");
                await Shell.Current.DisplayAlertAsync("Error", exception.Message, "OK");
            }
        }

        private async System.Threading.Tasks.Task CreateCourseAsync()
        {
            if (SelectedTeacher == null ||
                string.IsNullOrWhiteSpace(CourseName) ||
                string.IsNullOrWhiteSpace(CourseSubject))
            {
                await Shell.Current.DisplayAlertAsync("Error", "Complete all fields", "OK");
                return;
            }

            try
            {
                await _api.CreateCourseAsync(CourseName, CourseSubject, SelectedTeacher.Id);
                await Shell.Current.DisplayAlertAsync("Success", "Course created", "OK");
                CourseName = string.Empty;
                CourseSubject = string.Empty;
                await LoadDataAsync();
                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        private async System.Threading.Tasks.Task CreateTeacherAsync()
        {
            if (string.IsNullOrWhiteSpace(TeacherName) ||
                string.IsNullOrWhiteSpace(TeacherEmail) ||
                string.IsNullOrWhiteSpace(TeacherPassword))
            {
                await Shell.Current.DisplayAlertAsync("Error", "Complete all fields", "OK");
                return;
            }

            try
            {
                await _api.RegisterAsync(
                    TeacherName, TeacherEmail, TeacherPassword,
                    "Professor", "NOTIONFLOW_ADMIN_2024");

                await Shell.Current.DisplayAlertAsync("Success", "Teacher created successfully", "OK");
                TeacherName = string.Empty;
                TeacherEmail = string.Empty;
                TeacherPassword = string.Empty;
                await LoadDataAsync();
                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        private async Task AssignStudentAsync()
        {
            if (SelectedCourse == null || SelectedStudent == null)
            {
                await Shell.Current.DisplayAlertAsync("Error", "Select a course and a student", "OK");
                return;
            }

            try
            {
                await _api.AssignStudentAsync(SelectedCourse.Id, SelectedStudent.Id);
                await Shell.Current.DisplayAlertAsync("Success",
                    $"{SelectedStudent.Name} assigned to {SelectedCourse.Name}", "OK");
                await LoadDataAsync();
            }
            catch (Exception exception)
            {
                await Shell.Current.DisplayAlertAsync("Error", exception.Message, "OK");
            }
        }

        private async Task LogoutAsync()
        {
            await AuthService.LogoutAsync();
            await Shell.Current.GoToAsync("//login");
        }
    }
}
