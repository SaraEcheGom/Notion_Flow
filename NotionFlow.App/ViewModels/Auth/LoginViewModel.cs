using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Diagnostics;
using NotionFlow.App.Services;

namespace NotionFlow.App.ViewModels.Auth
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private string _email = string.Empty;
        private string _password = string.Empty;

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        public LoginViewModel(AuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            LoginCommand = new Command(async () => await LoginAsync());
            NavigateToRegisterCommand = new Command(async () => await NavigateToRegisterAsync());
            Debug.WriteLine("✓ LoginViewModel initialized");
        }

        private async Task LoginAsync()
        {
            Debug.WriteLine("🔍 [LoginViewModel] Starting LoginAsync");

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                Debug.WriteLine("✗ [LoginViewModel] Email or password empty");
                await Shell.Current.DisplayAlert("Error", "Enter email and password", "OK");
                return;
            }

            Debug.WriteLine($"📧 [LoginViewModel] Email: {Email}");
            Debug.WriteLine($"🔑 [LoginViewModel] Password entered (length: {Password.Length})");

            try
            {
                Debug.WriteLine("🔐 [LoginViewModel] Calling AuthService.LoginAsync");
                var user = await _authService.LoginAsync(Email, Password);

                Debug.WriteLine($"✓ [LoginViewModel] Login successful. Role: {user.Role}");

                // The API issues role names "Admin" / "Professor" / "Student" (see
                // DataSeeder + AuthController). The Shell routes registered in
                // AppShell.xaml are "admin" / "teacher" / "estudiante". Match
                // case-insensitively and accept both English and Spanish spellings so
                // navigation never silently no-ops, and use the routes that actually
                // exist (the previous //professor and //student routes were not
                // registered anywhere).
                var role = (user.Role ?? string.Empty).Trim();

                if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine("→ [LoginViewModel] Navigating to //admin");
                    await Shell.Current.GoToAsync("//admin");
                }
                else if (string.Equals(role, "Professor", StringComparison.OrdinalIgnoreCase)
                      || string.Equals(role, "Profesor", StringComparison.OrdinalIgnoreCase)
                      || string.Equals(role, "Teacher", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine($"→ [LoginViewModel] Navigating to //teacher");
                    await Shell.Current.GoToAsync($"//teacher?id={user.Id}");
                }
                else if (string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase)
                      || string.Equals(role, "Estudiante", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine($"→ [LoginViewModel] Navigating to //estudiante");
                    await Shell.Current.GoToAsync($"//estudiante?id={user.Id}");
                }
                else
                {
                    Debug.WriteLine($"✗ [LoginViewModel] Unknown role '{role}' — no navigation");
                    await Shell.Current.DisplayAlert(
                        "Login",
                        $"El usuario inició sesión correctamente pero su rol ('{role}') no está mapeado a ninguna pantalla.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✗ [LoginViewModel] Exception: {ex.GetType().Name}");
                Debug.WriteLine($"✗ [LoginViewModel] Message: {ex.Message}");
                Debug.WriteLine($"✗ [LoginViewModel] StackTrace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task NavigateToRegisterAsync()
        {
            Debug.WriteLine("🔍 [LoginViewModel] Navigating to register page");
            await Shell.Current.GoToAsync("register");
        }
    }
}