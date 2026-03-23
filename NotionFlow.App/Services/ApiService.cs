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

        public ApiService()
        {
            // Determine base URL based on platform
            _baseUrl = GetApiBaseUrl();
            Debug.WriteLine($"🌐 [ApiService] BaseUrl: {_baseUrl}");

            // Configure HttpClient with timeout and handler
            var handler = new HttpClientHandler();

#if DEBUG
            // In development, allow HTTP connections without HTTPS
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

            _httpClient = new HttpClient(handler) 
            { 
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
            SetAuthorizationHeader();
            Debug.WriteLine("✓ ApiService initialized");
        }

        private static string GetApiBaseUrl()
        {
            // Android emulator: 10.0.2.2 = host machine
            // iOS: localhost or IP address
            // Windows: WSL2 IP address

#if __ANDROID__
            return "http://10.0.2.2:5000/api/";
#elif __IOS__
            return "http://localhost:5000/api/";
#elif WINDOWS
            // Use WSL2 IP from Windows
            return "http://172.17.218.15:5000/api/";
#else
            return "http://localhost:5000/api/";
#endif
        }

        private void SetAuthorizationHeader()
        {
            var token = Preferences.Get("jwt_token", string.Empty);
            if (!string.IsNullOrEmpty(token))
            {
                Debug.WriteLine("🔑 [ApiService] JWT token found in Preferences");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                Debug.WriteLine("⚠️ [ApiService] No JWT token in Preferences");
            }
        }

        private static StringContent CreateJsonContent(object obj) =>
            new StringContent(
                JsonSerializer.Serialize(obj),
                Encoding.UTF8,
                "application/json");

        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            Debug.WriteLine($"📡 [ApiService] Starting POST to auth/login");
            Debug.WriteLine($"📧 [ApiService] Email: {email}");

            try
            {
                var response = await _httpClient.PostAsync("auth/login",
                    CreateJsonContent(new { email, password }));

                Debug.WriteLine($"📊 [ApiService] Status Code: {response.StatusCode}");

                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"📄 [ApiService] JSON received (length: {jsonContent.Length})");

                var data = JsonSerializer.Deserialize<AuthResponse>(jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

                Debug.WriteLine($"✓ [ApiService] AuthResponse deserialized. Token: {data.Token.Substring(0, 20)}...");

                Preferences.Set("jwt_token", data.Token);
                SetAuthorizationHeader();

                Debug.WriteLine($"✓ [ApiService] Token saved to Preferences");
                return data;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"✗ [ApiService] HttpRequestException: {ex.Message}");
                Debug.WriteLine($"✗ [ApiService] InnerException: {ex.InnerException?.Message}");
                throw new Exception($"Connection error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✗ [ApiService] Exception: {ex.GetType().Name} - {ex.Message}");
                throw;
            }
        }

        public async Task<List<CourseResponse>> GetCoursesForAdminAsync()
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync("courses");
            response.EnsureSuccessStatusCode();
            var jsonContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CourseResponse>>(jsonContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
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
            try
            {
                SetAuthorizationHeader();
                var endpoint = $"auth/users?role={role}";
                var fullUrl = $"{_baseUrl}{endpoint}";
                Debug.WriteLine($"🔍 [ApiService] GET request to: {fullUrl}");
                Debug.WriteLine($"🔐 [ApiService] Authorization header: {_httpClient.DefaultRequestHeaders.Authorization?.Scheme} {(_httpClient.DefaultRequestHeaders.Authorization?.Parameter != null ? "***" : "MISSING")}");

                var response = await _httpClient.GetAsync(endpoint);
                Debug.WriteLine($"📊 [ApiService] Response status: {(int)response.StatusCode} {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"✗ [ApiService] Error response body: {errorContent}");
                }

                response.EnsureSuccessStatusCode();
                var jsonContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<List<AuthResponse>>(jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                Debug.WriteLine($"✓ [ApiService] GetUsersByRoleAsync('{role}') returned {result.Count} users");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✗ [ApiService] GetUsersByRoleAsync('{role}') exception: {ex.GetType().Name}");
                Debug.WriteLine($"✗ [ApiService] Message: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CourseResponse>> GetCoursesByProfessorAsync(string professorId)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync($"courses/professor/{professorId}");
            response.EnsureSuccessStatusCode();
            var jsonContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CourseResponse>>(jsonContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async Task<List<CourseResponse>> GetCoursesByStudentAsync(string studentId)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync($"courses/student/{studentId}");
            response.EnsureSuccessStatusCode();
            var jsonContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CourseResponse>>(jsonContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async Task CreateCourseAsync(string name, string subject, string professorId)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.PostAsync("courses",
                CreateJsonContent(new { name, subject, professorId }));
            response.EnsureSuccessStatusCode();
        }

        public async Task AssignStudentAsync(int courseId, string studentId)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.PostAsync($"courses/{courseId}/students",
                CreateJsonContent(new { studentId }));
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<Evaluation>> GetEvaluationsAsync(int courseId)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync($"courses/{courseId}/evaluations");
            response.EnsureSuccessStatusCode();
            var jsonContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Evaluation>>(jsonContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async Task CreateEvaluationAsync(int courseId, string title,
            string description, double percentage)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.PostAsync($"courses/{courseId}/evaluations",
                CreateJsonContent(new { title, description, date = DateTime.Now, percentageValue = percentage }));
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<Content>> GetContentsAsync(int courseId)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync($"courses/{courseId}/contents");
            response.EnsureSuccessStatusCode();
            var jsonContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Content>>(jsonContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async Task PublishContentAsync(int courseId, string title,
            string description, string type, string url)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.PostAsync($"courses/{courseId}/contents",
                CreateJsonContent(new { title, description, type, url }));
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<CourseResponse>> GetAllCoursesAsync()
        {
            try
            {
                SetAuthorizationHeader();
                var endpoint = "courses";
                var fullUrl = $"{_baseUrl}{endpoint}";
                Debug.WriteLine($"🔍 [ApiService] GET request to: {fullUrl}");
                Debug.WriteLine($"🔐 [ApiService] Authorization header: {_httpClient.DefaultRequestHeaders.Authorization?.Scheme} {(_httpClient.DefaultRequestHeaders.Authorization?.Parameter != null ? "***" : "MISSING")}");

                var response = await _httpClient.GetAsync(endpoint);
                Debug.WriteLine($"📊 [ApiService] Response status: {(int)response.StatusCode} {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"✗ [ApiService] Error response body: {errorContent}");
                }

                response.EnsureSuccessStatusCode();
                var jsonContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<List<CourseResponse>>(jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                Debug.WriteLine($"✓ [ApiService] GetAllCoursesAsync() returned {result.Count} courses");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✗ [ApiService] GetAllCoursesAsync() exception: {ex.GetType().Name}");
                Debug.WriteLine($"✗ [ApiService] Message: {ex.Message}");
                throw;
            }
        }
    }
}