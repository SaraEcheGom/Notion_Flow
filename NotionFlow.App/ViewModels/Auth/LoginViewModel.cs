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

                if (user.Role == "Admin")
                {
                    Debug.WriteLine("→ [LoginViewModel] Navigating to //admin");
                    await Shell.Current.GoToAsync("//admin");
                }
                else if (user.Role == "Profesor")
                {
                    Debug.WriteLine($"→ [LoginViewModel] Navigating to //professor");
                    await Shell.Current.GoToAsync($"//professor?id={user.Id}&name={user.Name}");
                }
                else if (user.Role == "Estudiante")
                {
                    Debug.WriteLine($"→ [LoginViewModel] Navigating to //student");
                    await Shell.Current.GoToAsync($"//student?id={user.Id}");
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