using System.Collections.ObjectModel;
using System.Windows.Input;
using NotionFlow.App.Constants;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;
using NotionFlow.App.Views.Admin;
using NotionFlow.App.Views.Auth;
using NotionFlow.App.Views.Course;
using NotionFlow.App.ViewModels.Auth;
using NotionFlow.App.ViewModels.Course;

namespace NotionFlow.App.ViewModels.Admin
{
    public class AdminViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;

        public ObservableCollection<AuthResponse> Teachers { get; } = new();
        public ObservableCollection<AuthResponse> Students { get; } = new();
        public ObservableCollection<CourseResponse> Courses { get; } = new();

        private AuthResponse? _selectedTeacher;
        private AuthResponse? _selectedStudent;
        private CourseResponse? _selectedCourse;
        private string _courseName = string.Empty;
        private string _courseSubject = string.Empty;
        private string _courseDescription = string.Empty;
        private string _teacherName = string.Empty;
        private string _teacherEmail = string.Empty;
        private string _teacherPassword = string.Empty;
        private string _studentName = string.Empty;
        private string _studentEmail = string.Empty;
        private string _studentPassword = string.Empty;

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

        public string CourseDescription
        {
            get => _courseDescription;
            set { _courseDescription = value; OnPropertyChanged(); }
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

        public string StudentName
        {
            get => _studentName;
            set { _studentName = value; OnPropertyChanged(); }
        }

        public string StudentEmail
        {
            get => _studentEmail;
            set { _studentEmail = value; OnPropertyChanged(); }
        }

        public string StudentPassword
        {
            get => _studentPassword;
            set { _studentPassword = value; OnPropertyChanged(); }
        }

        public ICommand LoadDataCommand { get; }
        public ICommand CreateCourseCommand { get; }
        public ICommand AssignStudentCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand GoToCreateCourseCommand { get; }
        public ICommand GoToCreateTeacherCommand { get; }
        public ICommand GoToCreateStudentCommand { get; }
        public ICommand CreateTeacherCommand { get; }
        public ICommand CreateStudentCommand { get; }
        public ICommand ViewProfileCommand { get; }
        public ICommand ViewCourseDetailsCommand { get; }

        public AdminViewModel(ApiService apiService, AuthService authService)
        {
            _api = apiService;
            _auth = authService;

            LoadDataCommand = new Command(async () => await LoadDataAsync());
            CreateCourseCommand = new Command(async () => await CreateCourseAsync());
            AssignStudentCommand = new Command(async () => await AssignStudentAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());
            CreateTeacherCommand = new Command(async () => await CreateTeacherAsync());

            GoToCreateCourseCommand = new Command(async () =>
                await Shell.Current.Navigation.PushAsync(new CreateCoursePage(this)));

            GoToCreateTeacherCommand = new Command(async () =>
                await Shell.Current.Navigation.PushAsync(new CreateTeacherPage(this)));

            GoToCreateStudentCommand = new Command(async () =>
                await Shell.Current.Navigation.PushAsync(new CreateStudentPage(this)));

            CreateStudentCommand = new Command(async () => await CreateStudentAsync());

            ViewProfileCommand = new Command<AuthResponse>(async (u) =>
                await Shell.Current.Navigation.PushAsync(
                    new UserProfilePage(new UserProfileViewModel(u, _api))));

            ViewCourseDetailsCommand = new Command<CourseResponse>(async (course) =>
            {
                if (course == null) return;
                await Shell.Current.Navigation.PushAsync(
                    new CourseDetailsPage(course, _api, _auth));
            });

            _ = ValidateAndLoadDataAsync();
        }

        private async Task ValidateAndLoadDataAsync()
        {
            var currentUser = _auth.CurrentUser;

            if (currentUser == null)
            {
                await Shell.Current.DisplayAlert("Error", "User not authenticated. Please login again.", "OK");
                await Shell.Current.GoToAsync(Routes.Login);
                return;
            }

            if (currentUser.Role != Roles.Admin)
            {
                await Shell.Current.DisplayAlert("Error", "Only administrators can access this page.", "OK");
                await Shell.Current.GoToAsync(Routes.Login);
                return;
            }

            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var currentUser = _auth.CurrentUser;
                if (currentUser == null)
                {
                    await Shell.Current.DisplayAlert("Error", "User not authenticated", "OK");
                    return;
                }

                if (currentUser.InstitutionId <= 0)
                {
                    await Shell.Current.DisplayAlert("Error",
                        $"Invalid Institution ID ({currentUser.InstitutionId}). Please logout and login again.", "OK");
                    return;
                }

                var teachers = await _api.GetUsersByRoleAsync(Roles.Professor);
                var students = await _api.GetUsersByRoleAsync(Roles.Student);
                var courses = await _api.GetAllCoursesAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Teachers.Clear();
                    foreach (var t in teachers) Teachers.Add(t);

                    Students.Clear();
                    foreach (var s in students) Students.Add(s);

                    Courses.Clear();
                    foreach (var c in courses) Courses.Add(c);
                });
            }
            catch (Exception ex)
            {
                CrashLog.Write("AdminViewModel.LoadDataAsync", ex);
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task CreateCourseAsync()
        {
            if (SelectedTeacher == null ||
                string.IsNullOrWhiteSpace(CourseName) ||
                string.IsNullOrWhiteSpace(CourseSubject) ||
                string.IsNullOrWhiteSpace(CourseDescription))
            {
                await Shell.Current.DisplayAlert("Error", "Complete all fields", "OK");
                return;
            }

            try
            {
                await _api.CreateCourseAsync(CourseName, CourseSubject, CourseDescription, SelectedTeacher.Id);
                await Shell.Current.DisplayAlert("Success", "Course created", "OK");
                CourseName = string.Empty;
                CourseSubject = string.Empty;
                CourseDescription = string.Empty;
                await LoadDataAsync();
                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                CrashLog.Write("AdminViewModel.CreateCourse", ex);
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task CreateTeacherAsync()
        {
            if (string.IsNullOrWhiteSpace(TeacherName) ||
                string.IsNullOrWhiteSpace(TeacherEmail) ||
                string.IsNullOrWhiteSpace(TeacherPassword))
            {
                await Shell.Current.DisplayAlert("Error", "Complete all fields", "OK");
                return;
            }

            try
            {
                await _api.RegisterAsync(TeacherName, TeacherEmail, TeacherPassword, Roles.Professor, "ADMIN");
                await Shell.Current.DisplayAlert("Success", "Teacher created successfully", "OK");
                TeacherName = string.Empty;
                TeacherEmail = string.Empty;
                TeacherPassword = string.Empty;
                await LoadDataAsync();
                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                CrashLog.Write("AdminViewModel.CreateTeacher", ex);
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task CreateStudentAsync()
        {
            if (string.IsNullOrWhiteSpace(StudentName) ||
                string.IsNullOrWhiteSpace(StudentEmail) ||
                string.IsNullOrWhiteSpace(StudentPassword))
            {
                await Shell.Current.DisplayAlert("Error", "Complete all fields", "OK");
                return;
            }

            try
            {
                await _api.RegisterAsync(StudentName, StudentEmail, StudentPassword, Roles.Student, "ADMIN");
                await Shell.Current.DisplayAlert("Success", "Student created successfully", "OK");
                StudentName = string.Empty;
                StudentEmail = string.Empty;
                StudentPassword = string.Empty;
                await LoadDataAsync();
                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                CrashLog.Write("AdminViewModel.CreateStudent", ex);
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task AssignStudentAsync()
        {
            if (SelectedCourse == null)
            {
                await Shell.Current.DisplayAlert("Error", "Select a course", "OK");
                return;
            }

            if (SelectedStudent == null)
            {
                await Shell.Current.DisplayAlert("Error", "Select a student", "OK");
                return;
            }

            try
            {
                await _api.AssignStudentAsync(SelectedCourse.Id, SelectedStudent.Id);
                await Shell.Current.DisplayAlert("Success",
                    $"{SelectedStudent.Name} has been assigned to {SelectedCourse.Name}", "OK");
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                CrashLog.Write("AdminViewModel.AssignStudent", ex);
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task LogoutAsync()
        {
            await _auth.LogoutAsync();

            if (Application.Current?.MainPage is AppShell shell)
                await shell.LogoutAsync();
            else
                await Shell.Current.GoToAsync(Routes.Login);
        }
    }
}
