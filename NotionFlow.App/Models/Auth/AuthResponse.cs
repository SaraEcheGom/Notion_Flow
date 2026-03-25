using System.Text.Json.Serialization;

namespace NotionFlow.App.Models.Auth
{
    public class AuthResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("institutionId")]
        public int InstitutionId { get; set; }
    }

    public class CourseResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("teacherId")]
        public string TeacherId { get; set; } = string.Empty;

        [JsonPropertyName("teacherName")]
        public string TeacherName { get; set; } = string.Empty;

        [JsonPropertyName("students")]
        public List<StudentItem> Students { get; set; } = new();
    }

    public class StudentItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("nombre")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
    }

    public class Evaluation
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("titulo")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("descripcion")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("porcentajeValor")]
        public double PercentageValue { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("titulo")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("descripcion")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("tipo")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}
