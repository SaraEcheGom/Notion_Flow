using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NotionFlow.App.Models;
using NotionFlow.App.Models.Auth;

namespace NotionFlow.App.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        private static readonly JsonSerializerOptions JsonOptions =
            new() { PropertyNameCaseInsensitive = true };

        public ApiService()
        {
            _baseUrl = GetApiBaseUrl();

            var handler = new HttpClientHandler();

#if DEBUG
            // Permite certificados self-signed solo en debug
            handler.ServerCertificateCustomValidationCallback =
                (message, cert, chain, errors) => true;
#endif

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        private static string GetApiBaseUrl()
        {
#if __ANDROID__
            return "http://10.0.2.2:5000/api/";
#elif __IOS__
            return "http://localhost:5000/api/";
#elif WINDOWS
            return "http://127.0.0.1:5000/api/";
#else
            return "http://127.0.0.1:5000/api/";
#endif
        }

        /// <summary>Carga el JWT desde SecureStorage y configura el header Authorization.</summary>
        public async Task RefreshAuthHeaderAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync("jwt_token");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Error leyendo token seguro: {ex.GetType().Name}");
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        private static StringContent CreateJsonContent(object obj) =>
            new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsync("auth/login",
                    CreateJsonContent(new { email, password }));

                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<AuthResponse>(jsonContent, JsonOptions)!;

                // Guardar token en SecureStorage (cifrado por el OS)
                await SecureStorage.SetAsync("jwt_token", data.Token);
                await RefreshAuthHeaderAsync();

                return data;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error de conexión: {ex.Message}", ex);
            }
        }

        public async Task<List<CourseResponse>> GetCoursesForAdminAsync()
        {
            await RefreshAuthHeaderAsync();
            var response = await _httpClient.GetAsync("courses");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CourseResponse>>(json, JsonOptions)!;
        }

        public async Task RegisterAsync(string name, string email,
            string password, string role, string token)
        {
            var response = await _httpClient.PostAsync("auth/register",
                CreateJsonContent(new { name, email, password, role, token }));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception(errorContent);
            }
        }

        public async Task<List<AuthResponse>> GetUsersByRoleAsync(string role)
        {
            await RefreshAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"auth/users?role={role}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error {(int)response.StatusCode}: {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<AuthResponse>>(json, JsonOptions)!;
        }

        public async Task<List<CourseResponse>> GetCoursesByProfessorAsync(string professorId)
        {
            await RefreshAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"courses/teacher/{professorId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CourseResponse>>(json, JsonOptions)!;
        }

        public async Task<List<CourseResponse>> GetCoursesByStudentAsync(string studentId)
        {
            await RefreshAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"courses/student/{studentId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CourseResponse>>(json, JsonOptions)!;
        }

        public async Task CreateCourseAsync(string name, string subject, string description, string teacherId)
        {
            await RefreshAuthHeaderAsync();
            var response = await _httpClient.PostAsync("courses",
                CreateJsonContent(new { Name = name, Subject = subject, Description = description, TeacherId = teacherId }));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al crear curso: {errorContent}");
            }
        }

        public async Task AssignStudentAsync(int courseId, string studentId)
        {
            await RefreshAuthHeaderAsync();
            var response = await _httpClient.PostAsync(
                $"courses/{courseId}/students",
                CreateJsonContent(new { studentId }));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error {(int)response.StatusCode}: {errorContent}");
            }
        }

        public async Task<List<Evaluation>> GetEvaluationsAsync(int courseId)
        {
            await RefreshAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"courses/{courseId}/evaluations");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Evaluation>>(json, JsonOptions)!;
        }

        public async Task CreateEvaluationAsync(int courseId, string title,
            string description, double percentage)
        {
            await RefreshAuthHeaderAsync();
            var response = await _httpClient.PostAsync($"courses/{courseId}/evaluations",
                CreateJsonContent(new { title, description, date = DateTime.Now, percentageValue = percentage }));
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<Content>> GetContentsAsync(int courseId)
        {
            await RefreshAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"courses/{courseId}/contents");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Content>>(json, JsonOptions)!;
        }

        public async Task RemoveStudentAsync(int courseId, string studentId)
        {
            await RefreshAuthHeaderAsync();
            var response = await _httpClient.DeleteAsync($"courses/{courseId}/students/{studentId}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error {(int)response.StatusCode}: {errorContent}");
            }
        }

        public async Task<List<ActivityModel>> GetActivitiesAsync(int courseId)
        {
            await RefreshAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"courses/{courseId}/activities");
            if (!response.IsSuccessStatusCode)
                return new List<ActivityModel>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ActivityModel>>(json, JsonOptions)
                ?? new List<ActivityModel>();
        }

        public async Task<ActivityModel> CreateActivityAsync(int courseId, object activityPayload)
        {
            await RefreshAuthHeaderAsync();
            var response = await _httpClient.PostAsync(
                $"courses/{courseId}/activities",
                CreateJsonContent(activityPayload));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al crear actividad: {error}");
            }
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ActivityModel>(json, JsonOptions)!;
        }

        public async Task<ActivityModel> UpdateActivityAsync(int courseId, int activityId, object activityPayload)
        {
            await RefreshAuthHeaderAsync();
            var response = await _httpClient.PutAsync(
                $"courses/{courseId}/activities/{activityId}",
                CreateJsonContent(activityPayload));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al editar actividad: {error}");
            }
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ActivityModel>(json, JsonOptions)!;
        }

        public async Task DeleteActivityAsync(int courseId, int activityId)
        {
            await RefreshAuthHeaderAsync();
            var response = await _httpClient.DeleteAsync(
                $"courses/{courseId}/activities/{activityId}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al eliminar actividad: {error}");
            }
        }

        public async Task AssignActivityAsync(int courseId, int activityId, List<string> studentIds)
        {
            await RefreshAuthHeaderAsync();
            var response = await _httpClient.PostAsync(
                $"courses/{courseId}/activities/{activityId}/assign",
                CreateJsonContent(new { studentIds }));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al asignar actividad: {error}");
            }
        }

        public async Task<SubmitFeedbackResponse> SubmitActivityAsync(int courseId, int activityId,
            List<NotionFlow.App.Views.Student.AnswerPayload> answers)
        {
            await RefreshAuthHeaderAsync();
            var payload = answers.Select(a => new
            {
                questionId = a.QuestionId,
                selectedOptionIds = a.SelectedOptionIds,
                textAnswer = a.TextAnswer,
            }).ToList();
            var response = await _httpClient.PostAsync(
                $"courses/{courseId}/activities/{activityId}/submit",
                CreateJsonContent(new { answers = payload }));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al enviar actividad: {error}");
            }
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SubmitFeedbackResponse>(json, JsonOptions)
                ?? new SubmitFeedbackResponse();
        }

        public async Task<ActivityResultsResponse> GetActivityResultsAsync(int courseId, int activityId)
        {
            await RefreshAuthHeaderAsync();
            var response = await _httpClient.GetAsync(
                $"courses/{courseId}/activities/{activityId}/results");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al obtener resultados: {error}");
            }
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ActivityResultsResponse>(json, JsonOptions)
                ?? new ActivityResultsResponse();
        }

        public async Task PublishContentAsync(int courseId, string title,
            string description, string type, string url)
        {
            await RefreshAuthHeaderAsync();
            var response = await _httpClient.PostAsync($"courses/{courseId}/contents",
                CreateJsonContent(new { title, description, type, url }));
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<CourseResponse>> GetAllCoursesAsync()
        {
            await RefreshAuthHeaderAsync();
            var response = await _httpClient.GetAsync("courses");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error {(int)response.StatusCode}: {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CourseResponse>>(json, JsonOptions)!;
        }
    }
}
