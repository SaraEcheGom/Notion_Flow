using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Diagnostics;
using NotionFlow.App.Services;
using NotionFlow.App.Views.Admin;
using NotionFlow.App.Views.Auth;
using NotionFlow.App.Views.Course;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.ViewModels.Auth;
using NotionFlow.App.ViewModels.Course;

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
                await Shell.Current.Navigation.PushAsync(new CreateTeacherPage(this)));

            GoToCreateStudentCommand = new Command(async () =>
                await Shell.Current.Navigation.PushAsync(new CreateStudentPage(this)));

            CreateStudentCommand = new Command(async () => await CreateStudentAsync());

            ViewProfileCommand = new Command<AuthResponse>(async (u) =>
                await Shell.Current.Navigation.PushAsync(
                    new UserProfilePage(new UserProfileViewModel(u))));

            ViewCourseDetailsCommand = new Command<CourseResponse>(async (course) =>
            {
                if (course == null) return;
                await Shell.Current.Navigation.PushAsync(
                    new CourseDetailsPage(course, _api));
            });

            Debug.WriteLine("✓ [AdminViewModel] Constructor initialized");
            _ = ValidateAndLoadDataAsync();
        }

        private async Task ValidateAndLoadDataAsync()
        {
            Debug.WriteLine("\n🔐 [AdminViewModel] Validating user role...");
            var currentUser = AuthService.CurrentUser;

            if (currentUser == null)
            {
                Debug.WriteLine("❌ [AdminViewModel] CurrentUser is null - user not authenticated");
                await Shell.Current.DisplayAlertAsync("Error", "User not authenticated. Please login again.", "OK");
                await Shell.Current.GoToAsync("//login");
                return;
            }

            Debug.WriteLine($"✓ [AdminViewModel] CurrentUser: {currentUser.Name}, ID: {currentUser.Id}");
            Debug.WriteLine($"  Role: {currentUser.Role}");
            Debug.WriteLine($"  Institution: {currentUser.InstitutionId}");

            if (currentUser.Role != "Admin")
            {
                Debug.WriteLine($"❌ [AdminViewModel] User role is '{currentUser.Role}', but 'Admin' is required");
                await Shell.Current.DisplayAlertAsync("Error", "Only administrators can access this page.", "OK");
                await Shell.Current.GoToAsync("//login");
                return;
            }

            Debug.WriteLine("✓ [AdminViewModel] User is authenticated and has Admin role");
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                Debug.WriteLine("📊 [AdminViewModel] Starting LoadDataAsync");

                // Get current admin's InstitutionId
                var currentUser = AuthService.CurrentUser;
                if (currentUser == null)
                {
                    Debug.WriteLine("⚠️ [AdminViewModel] CurrentUser is null");
                    await Shell.Current.DisplayAlertAsync("Error", "User not authenticated", "OK");
                    return;
                }

                Debug.WriteLine($"👤 [AdminViewModel] CurrentUser: {currentUser.Name}, ID: {currentUser.Id}, Institution: {currentUser.InstitutionId}");

                var adminInstitutionId = currentUser.InstitutionId;
                if (adminInstitutionId <= 0)
                {
                    Debug.WriteLine($"⚠️ [AdminViewModel] Invalid InstitutionId: {adminInstitutionId}. This may indicate a database sync issue.");
                    Debug.WriteLine("💡 [AdminViewModel] Hint: Check if the user was created with InstitutionId set in the database");
                    await Shell.Current.DisplayAlertAsync("Error", 
                        $"Invalid Institution ID ({adminInstitutionId}). Please logout and login again.", "OK");
                    return;
                }
                Debug.WriteLine($"🏢 [AdminViewModel] Loading data for Institution ID: {adminInstitutionId}");

                Debug.WriteLine("🔍 [AdminViewModel] Calling GetUsersByRoleAsync('Professor')");
                var teachers = await _api.GetUsersByRoleAsync("Professor");
                Debug.WriteLine($"✓ [AdminViewModel] Got {teachers.Count} teachers from API (filtered by institution)");

                Debug.WriteLine("🔍 [AdminViewModel] Calling GetUsersByRoleAsync('Student')");
                var students = await _api.GetUsersByRoleAsync("Student");
                Debug.WriteLine($"✓ [AdminViewModel] Got {students.Count} students from API (filtered by institution)");

                Debug.WriteLine("🔍 [AdminViewModel] Calling GetAllCoursesAsync()");
                var courses = await _api.GetAllCoursesAsync();
                Debug.WriteLine($"✓ [AdminViewModel] Got {courses.Count} courses from API (filtered by institution {adminInstitutionId})");

                foreach (var course in courses)
                {
                    Debug.WriteLine($"  - Course: {course.Name} (ID: {course.Id}, Subject: {course.Subject})");
                }

                Teachers.Clear();
                foreach (var teacher in teachers) Teachers.Add(teacher);

                Students.Clear();
                foreach (var student in students) Students.Add(student);

                Courses.Clear();
                foreach (var course in courses) Courses.Add(course);

                Debug.WriteLine($"✓ [AdminViewModel] LoadDataAsync completed. Teachers: {Teachers.Count}, Students: {Students.Count}, Courses: {Courses.Count}");
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
                string.IsNullOrWhiteSpace(CourseSubject) ||
                string.IsNullOrWhiteSpace(CourseDescription))
            {
                await Shell.Current.DisplayAlertAsync("Error", "Complete all fields", "OK");
                return;
            }

            try
            {
                await _api.CreateCourseAsync(CourseName, CourseSubject, CourseDescription, SelectedTeacher.Id);
                await Shell.Current.DisplayAlertAsync("Success", "Course created", "OK");
                CourseName = string.Empty;
                CourseSubject = string.Empty;
                CourseDescription = string.Empty;
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
                    "Professor", "ADMIN");

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

        private async System.Threading.Tasks.Task CreateStudentAsync()
        {
            if (string.IsNullOrWhiteSpace(StudentName) ||
                string.IsNullOrWhiteSpace(StudentEmail) ||
                string.IsNullOrWhiteSpace(StudentPassword))
            {
                await Shell.Current.DisplayAlertAsync("Error", "Complete all fields", "OK");
                return;
            }

            try
            {
                await _api.RegisterAsync(
                    StudentName, StudentEmail, StudentPassword,
                    "Student", "ADMIN");

                await Shell.Current.DisplayAlertAsync("Success", "Student created successfully", "OK");
                StudentName = string.Empty;
                StudentEmail = string.Empty;
                StudentPassword = string.Empty;
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
            Debug.WriteLine("\n📝 [AdminViewModel] AssignStudentAsync called");

            if (SelectedCourse == null)
            {
                Debug.WriteLine("❌ [AdminViewModel] SelectedCourse is null");
                await Shell.Current.DisplayAlertAsync("Error", "Select a course", "OK");
                return;
            }

            if (SelectedStudent == null)
            {
                Debug.WriteLine("❌ [AdminViewModel] SelectedStudent is null");
                await Shell.Current.DisplayAlertAsync("Error", "Select a student", "OK");
                return;
            }

            Debug.WriteLine($"✓ [AdminViewModel] Course selected: {SelectedCourse.Name} (ID: {SelectedCourse.Id})");
            Debug.WriteLine($"✓ [AdminViewModel] Student selected: {SelectedStudent.Name} (ID: {SelectedStudent.Id})");
            Debug.WriteLine($"  Student email: {SelectedStudent.Email}");
            Debug.WriteLine($"  Student role: {SelectedStudent.Role}");

            try
            {
                Debug.WriteLine($"📤 [AdminViewModel] Calling API.AssignStudentAsync...");
                await _api.AssignStudentAsync(SelectedCourse.Id, SelectedStudent.Id);

                Debug.WriteLine($"✅ [AdminViewModel] Assignment successful");
                await Shell.Current.DisplayAlertAsync("Success",
                    $"{SelectedStudent.Name} has been assigned to {SelectedCourse.Name}", "OK");

                await LoadDataAsync();
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"❌ [AdminViewModel] Error: {exception.GetType().Name}");
                Debug.WriteLine($"   Message: {exception.Message}");
                Debug.WriteLine($"   StackTrace: {exception.StackTrace}");
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
