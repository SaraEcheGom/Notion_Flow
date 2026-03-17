using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NotionFlow.App.Models;

namespace NotionFlow.App.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private const string BaseUrl = "http://localhost:5220/api/";

        public ApiService()
        {
            _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            SetAuth();
        }

        private void SetAuth()
        {
            var token = Preferences.Get("jwt_token", string.Empty);
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }

        private static StringContent Json(object obj) =>
            new StringContent(
                JsonSerializer.Serialize(obj),
                Encoding.UTF8,
                "application/json");

        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            var res = await _http.PostAsync("auth/login",
                Json(new { email, password }));

            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<AuthResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            Preferences.Set("jwt_token", data.Token);
            SetAuth();
            return data;
        }

        public async Task<List<CursoResponse>> GetCursosAdminAsync()
        {
            SetAuth();
            var res = await _http.GetAsync("cursos");
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CursoResponse>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async Task RegisterAsync(string nombre, string email,
            string password, string rol, string token)
        {
            var res = await _http.PostAsync("auth/register",
                Json(new { nombre, email, password, rol, token }));

            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();
                throw new Exception(err);
            }
        }

        public async Task<List<AuthResponse>> GetUsuariosPorRolAsync(string rol)
        {
            SetAuth();
            var res = await _http.GetAsync($"auth/usuarios?rol={rol}");
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<AuthResponse>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async Task<List<CursoResponse>> GetCursosProfesorAsync(string profesorId)
        {
            SetAuth();
            var res = await _http.GetAsync($"cursos/profesor/{profesorId}");
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CursoResponse>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async Task<List<CursoResponse>> GetCursosEstudianteAsync(string estudianteId)
        {
            SetAuth();
            var res = await _http.GetAsync($"cursos/estudiante/{estudianteId}");
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CursoResponse>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async Task CrearCursoAsync(string nombre, string materia, string profesorId)
        {
            SetAuth();
            var res = await _http.PostAsync("cursos",
                Json(new { nombre, materia, profesorId }));
            res.EnsureSuccessStatusCode();
        }

        public async Task AsignarEstudianteAsync(int cursoId, string estudianteId)
        {
            SetAuth();
            var res = await _http.PostAsync($"cursos/{cursoId}/estudiantes",
                Json(new { estudianteId }));
            res.EnsureSuccessStatusCode();
        }

        public async Task<List<Evaluacion>> GetEvaluacionesAsync(int cursoId)
        {
            SetAuth();
            var res = await _http.GetAsync($"cursos/{cursoId}/evaluaciones");
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Evaluacion>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async Task CrearEvaluacionAsync(int cursoId, string titulo,
            string descripcion, double porcentaje)
        {
            SetAuth();
            var res = await _http.PostAsync($"cursos/{cursoId}/evaluaciones",
                Json(new { titulo, descripcion, fecha = DateTime.Now, porcentajeValor = porcentaje }));
            res.EnsureSuccessStatusCode();
        }

        public async Task<List<Contenido>> GetContenidosAsync(int cursoId)
        {
            SetAuth();
            var res = await _http.GetAsync($"cursos/{cursoId}/contenidos");
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Contenido>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async Task PublicarContenidoAsync(int cursoId, string titulo,
            string descripcion, string tipo, string url)
        {
            SetAuth();
            var res = await _http.PostAsync($"cursos/{cursoId}/contenidos",
                Json(new { titulo, descripcion, tipo, url }));
            res.EnsureSuccessStatusCode();
        }

        public async Task<List<CursoResponse>> GetTodosLosCursosAsync()
        {
            SetAuth();
            var res = await _http.GetAsync("cursos");
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CursoResponse>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }
    }
}