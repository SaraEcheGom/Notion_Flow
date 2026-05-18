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

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
    }

    public class Evaluation
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("percentageValue")]
        public double PercentageValue { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }

    public class ActivityModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("courseId")]
        public int CourseId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("dueDate")]
        public DateTime DueDate { get; set; }

        [JsonPropertyName("percentageValue")]
        public double PercentageValue { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("questions")]
        public List<ActivityQuestionModel> Questions { get; set; } = new();

        [JsonPropertyName("assignments")]
        public List<ActivityAssignmentModel> Assignments { get; set; } = new();
    }

    public class ActivityQuestionModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("questionType")]
        public string QuestionType { get; set; } = "MultipleChoice";

        [JsonPropertyName("options")]
        public List<ActivityOptionModel> Options { get; set; } = new();
    }

    public class ActivityOptionModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("isCorrect")]
        public bool IsCorrect { get; set; }
    }

    public class ActivityAssignmentModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("studentId")]
        public string StudentId { get; set; } = string.Empty;

        [JsonPropertyName("assignedAt")]
        public DateTime AssignedAt { get; set; }
    }
}
