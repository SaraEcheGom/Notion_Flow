using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NotionFlow.App.Services;

namespace NotionFlow.App.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private string _nombre = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _rolSeleccionado = "Estudiante";
        private string _token = string.Empty;
        private bool _mostrarToken = false;

        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(); }
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

        public bool MostrarToken
        {
            get => _mostrarToken;
            set { _mostrarToken = value; OnPropertyChanged(); }
        }

        public string RolSeleccionado
        {
            get => _rolSeleccionado;
            set
            {
                _rolSeleccionado = value;
                OnPropertyChanged();
                MostrarToken = value == "Admin" || value == "Profesor";
            }
        }

        public ObservableCollection<string> Roles { get; } = new()
        {
            "Estudiante", "Profesor", "Admin"
        };

        public ICommand RegisterCommand { get; }

        public RegisterViewModel()
        {
            RegisterCommand = new Command(async () => await RegisterAsync());
        }

        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Nombre) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlert("Error", "Completa todos los campos", "OK");
                return;
            }

            try
            {
                await _authService.RegistrarAsync(Nombre, Email, Password, RolSeleccionado, Token);
                await Shell.Current.DisplayAlert("Cuenta creada", "Ahora puedes iniciar sesión", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}