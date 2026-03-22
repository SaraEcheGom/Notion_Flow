using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading.Tasks;
using NotionFlow.App.Services;

namespace NotionFlow.App.ViewModels.Auth
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private string _name = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _selectedRole = "Student";
        private string _token = string.Empty;
        private bool _showToken = false;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

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

        public string Token
        {
            get => _token;
            set { _token = value; OnPropertyChanged(); }
        }

        public bool ShowToken
        {
            get => _showToken;
            set { _showToken = value; OnPropertyChanged(); }
        }

        public string SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged();
                ShowToken = value == "Admin" || value == "Teacher";
            }
        }

        public ObservableCollection<string> Roles { get; } = new()
        {
            "Student", "Teacher", "Admin"
        };

        public ICommand RegisterCommand { get; }

        public RegisterViewModel()
        {
            RegisterCommand = new Command(async () => await RegisterAsync());
        }

        private async System.Threading.Tasks.Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlert("Error", "Complete all fields", "OK");
                return;
            }

            try
            {
                await _authService.RegisterAsync(Name, Email, Password, SelectedRole, Token);
                await Shell.Current.DisplayAlert("Account Created", "You can now login", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
