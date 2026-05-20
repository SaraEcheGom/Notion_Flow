using System.Collections.ObjectModel;
using System.Windows.Input;
using NotionFlow.App.Constants;
using NotionFlow.App.Services;

namespace NotionFlow.App.ViewModels.Auth
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

        private string _name = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _selectedRole = Constants.Roles.Student;
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
                ShowToken = value == Constants.Roles.Admin || value == Constants.Roles.Professor;
            }
        }

        public ObservableCollection<string> Roles { get; } = new()
        {
            Constants.Roles.Student,
            Constants.Roles.Professor,
            Constants.Roles.Admin
        };

        public ICommand RegisterCommand { get; }

        public RegisterViewModel(AuthService authService)
        {
            _authService = authService;
            RegisterCommand = new Command(async () => await RegisterAsync());
        }

        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlert("Error", "Completa todos los campos", "OK");
                return;
            }

            if (IsBusy) return;
            IsBusy = true;
            try
            {
                await _authService.RegisterAsync(Name, Email, Password, SelectedRole, Token);
                await Shell.Current.DisplayAlert("Cuenta creada", "Ya puedes iniciar sesión", "OK");    
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
