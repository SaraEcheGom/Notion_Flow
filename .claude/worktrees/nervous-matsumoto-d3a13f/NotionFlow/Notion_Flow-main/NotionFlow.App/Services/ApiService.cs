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
            // Windows: localhost (MAUI Windows runs as a native Windows process,
            //                     so docker-compose's published port 5000 is reachable
            //                     directly on the host).

            // Runtime override: lets the user point the app at any API URL without
            // recompiling. Set it once from anywhere with:
            //   Microsoft.Maui.Storage.Preferences.Set("api_base_url",
            //                                          "http://192.168.1.42:5000/api/");
            var overrideUrl = Preferences.Get("api_base_url", string.Empty);
            if (!string.IsNullOrWhiteSpace(overrideUrl))
                return overrideUrl;

#if __ANDROID__
            return "http://10.0.2.2:5000/api/";
#elif __IOS__
            return "http://localhost:5000/api/";
#elif WINDOWS
            // The previous hard-coded WSL2 IP (172.17.218.15) only worked on the
            // original developer's machine and rotates on every WSL2 reboot.
            // The user confirmed `Invoke-RestMethod http://localhost:5000/api/Auth/login`
            // works from PowerShell, so localhost is the correct value here.
            return "http://localhost:5000/api/";
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
                Debug.WriteLine($"📄 [ApiService] JSON Content: {jsonContent}");

                var data = JsonSerializer.Deserialize<AuthResponse>(jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

                Debug.WriteLine($"✓ [ApiService] AuthResponse deserialized:");
                Debug.WriteLine($"  - Token: {data.Token.Substring(0, Math.Min(20, data.Token.Length))}...");
                Debug.WriteLine($"  - Name: {data.Name}");
                Debug.WriteLine($"  - Email: {data.Email}");
                Debug.WriteLine($"  - Role: {data.Role}");
                Debug.WriteLine($"  - Id: {data.Id}");
                Debug.WriteLine($"  - InstitutionId: {data.InstitutionId}");

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
            // The API exposes [HttpGet("teacher/{teacherId}")] in CoursesController, not
            // "professor/{...}". The previous URL produced a 404 right after a Professor
            // logged in (TeacherViewModel calls this in its ctor), so the user saw the
            // error toast "status code doesnt indicate success 404 not found".
            var response = await _httpClient.GetAsync($"courses/teacher/{professorId}");
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

        public async Task CreateCourseAsync(string name, string subject, string description, string teacherId)
        {
            try
            {
                SetAuthorizationHeader();
                Debug.WriteLine($"🔍 [ApiService] Creating course: Name='{name}', Subject='{subject}', TeacherId='{teacherId}'");

                var payload = new { Name = name, Subject = subject, Description = description, TeacherId = teacherId };
                Debug.WriteLine($"📦 [ApiService] Payload: {JsonSerializer.Serialize(payload)}");

                var response = await _httpClient.PostAsync("courses",
                    CreateJsonContent(payload));

                Debug.WriteLine($"📊 [ApiService] Response status: {(int)response.StatusCode} {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"✗ [ApiService] Error response body: {errorContent}");
                }

                response.EnsureSuccessStatusCode();
                Debug.WriteLine($"✓ [ApiService] Course created successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✗ [ApiService] CreateCourseAsync exception: {ex.GetType().Name}");
                Debug.WriteLine($"✗ [ApiService] Message: {ex.Message}");
                throw;
            }
        }

        public async Task AssignStudentAsync(int courseId, string studentId)
        {
            try
            {
                SetAuthorizationHeader();
                var endpoint = $"courses/{courseId}/students";
                var fullUrl = $"{_baseUrl}{endpoint}";
                var payload = new { studentId };

                Debug.WriteLine($"\n📡 [ApiService] AssignStudentAsync called");
                Debug.WriteLine($"  Course ID: {courseId}");
                Debug.WriteLine($"  Student ID: {studentId}");
                Debug.WriteLine($"  URL: {fullUrl}");
                Debug.WriteLine($"  Payload: {JsonSerializer.Serialize(payload)}");
                Debug.WriteLine($"  Authorization header present: {_httpClient.DefaultRequestHeaders.Authorization != null}");

                var response = await _httpClient.PostAsync(endpoint,
                    CreateJsonContent(payload));

                Debug.WriteLine($"  Status Code: {(int)response.StatusCode} {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"✗ [ApiService] Error response body: {errorContent}");
                    throw new Exception($"API returned {(int)response.StatusCode}: {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"✓ [ApiService] AssignStudentAsync successful: {content}");
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"✗ [ApiService] HttpRequestException: {ex.Message}");
                throw new Exception($"Connection error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✗ [ApiService] Exception: {ex.GetType().Name} - {ex.Message}");
                throw;
            }
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

        public async Task RemoveStudentAsync(int courseId, string studentId)
        {
            try
            {
                SetAuthorizationHeader();
                var endpoint = $"courses/{courseId}/students/{studentId}";
                var fullUrl = $"{_baseUrl}{endpoint}";

                Debug.WriteLine($"\n📡 [ApiService] RemoveStudentAsync called");
                Debug.WriteLine($"  Course ID: {courseId}");
                Debug.WriteLine($"  Student ID: {studentId}");
                Debug.WriteLine($"  URL: {fullUrl}");
                Debug.WriteLine($"  Authorization header present: {_httpClient.DefaultRequestHeaders.Authorization != null}");

                var response = await _httpClient.DeleteAsync(endpoint);

                Debug.WriteLine($"  Status Code: {(int)response.StatusCode} {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"✗ [ApiService] Error response body: {errorContent}");
                    throw new Exception($"API returned {(int)response.StatusCode}: {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"✓ [ApiService] RemoveStudentAsync successful: {content}");
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"✗ [ApiService] HttpRequestException: {ex.Message}");
                throw new Exception($"Connection error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✗ [ApiService] Exception: {ex.GetType().Name} - {ex.Message}");
                throw;
            }
        }

        public async Task<List<ActivityModel>> GetActivitiesAsync(int courseId)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync($"courses/{courseId}/activities");
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[ApiService] GetActivitiesAsync {(int)response.StatusCode} {response.StatusCode}");
                return new List<ActivityModel>();   // devuelve lista vacía en vez de crash
            }
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ActivityModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<ActivityModel>();
        }

        public async Task<ActivityModel> CreateActivityAsync(int courseId, object activityPayload)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.PostAsync(
                $"courses/{courseId}/activities",
                CreateJsonContent(activityPayload));
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al crear actividad: {error}");
            }
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ActivityModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async Task<ActivityModel> UpdateActivityAsync(int courseId, int activityId, object activityPayload)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.PutAsync(
                $"courses/{courseId}/activities/{activityId}",
                CreateJsonContent(activityPayload));
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al editar actividad: {error}");
            }
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ActivityModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async Task DeleteActivityAsync(int courseId, int activityId)
        {
            SetAuthorizationHeader();
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
            SetAuthorizationHeader();
            var response = await _httpClient.PostAsync(
                $"courses/{courseId}/activities/{activityId}/assign",
                CreateJsonContent(new { studentIds }));
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al asignar actividad: {error}");
            }
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