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

    // ── Submit feedback (retroalimentación al estudiante) ────────────────────
    public class SubmitFeedbackResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("score")]
        public int? Score { get; set; }

        [JsonPropertyName("correct")]
        public int Correct { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("answers")]
        public List<QuestionFeedback> Answers { get; set; } = new();
    }

    public class QuestionFeedback
    {
        [JsonPropertyName("questionId")]
        public int QuestionId { get; set; }

        [JsonPropertyName("questionText")]
        public string QuestionText { get; set; } = string.Empty;

        [JsonPropertyName("questionType")]
        public string QuestionType { get; set; } = "MultipleChoice";

        [JsonPropertyName("isCorrect")]
        public bool IsCorrect { get; set; }

        [JsonPropertyName("selectedOptionIds")]
        public List<int> SelectedOptionIds { get; set; } = new();

        [JsonPropertyName("correctOptionIds")]
        public List<int> CorrectOptionIds { get; set; } = new();

        [JsonPropertyName("options")]
        public List<ActivityOptionModel> Options { get; set; } = new();

        [JsonPropertyName("textAnswer")]
        public string? TextAnswer { get; set; }
    }

    // ── HU#14 / HU#13: Progreso personal y por estudiante ───────────────────
    public class StudentProgressResponse
    {
        [JsonPropertyName("studentId")]
        public string StudentId { get; set; } = string.Empty;

        [JsonPropertyName("studentName")]
        public string StudentName { get; set; } = string.Empty;

        [JsonPropertyName("totalActivities")]
        public int TotalActivities { get; set; }

        [JsonPropertyName("completedActivities")]
        public int CompletedActivities { get; set; }

        [JsonPropertyName("averageScore")]
        public double AverageScore { get; set; }

        [JsonPropertyName("totalPoints")]
        public int TotalPoints { get; set; }

        [JsonPropertyName("levelName")]
        public string LevelName { get; set; } = string.Empty;

        [JsonPropertyName("levelEmoji")]
        public string LevelEmoji { get; set; } = string.Empty;

        [JsonPropertyName("nextLevelPoints")]
        public int NextLevelPoints { get; set; }

        [JsonPropertyName("streak")]
        public int Streak { get; set; }

        [JsonPropertyName("activityDetails")]
        public List<ActivityProgressDetail> ActivityDetails { get; set; } = new();

        [JsonPropertyName("badges")]
        public List<BadgeModel> Badges { get; set; } = new();
    }

    public class ActivityProgressDetail
    {
        [JsonPropertyName("activityId")]
        public int ActivityId { get; set; }

        [JsonPropertyName("activityTitle")]
        public string ActivityTitle { get; set; } = string.Empty;

        [JsonPropertyName("score")]
        public int? Score { get; set; }

        [JsonPropertyName("submittedAt")]
        public DateTime? SubmittedAt { get; set; }

        [JsonPropertyName("completed")]
        public bool Completed { get; set; }
    }

    // ── HU#15: Reporte general del curso ────────────────────────────────────
    public class CourseReportResponse
    {
        [JsonPropertyName("courseId")]
        public int CourseId { get; set; }

        [JsonPropertyName("courseName")]
        public string CourseName { get; set; } = string.Empty;

        [JsonPropertyName("totalStudents")]
        public int TotalStudents { get; set; }

        [JsonPropertyName("totalActivities")]
        public int TotalActivities { get; set; }

        [JsonPropertyName("averageCourseScore")]
        public double AverageCourseScore { get; set; }

        [JsonPropertyName("studentSummaries")]
        public List<StudentSummaryItem> StudentSummaries { get; set; } = new();
    }

    public class StudentSummaryItem
    {
        [JsonPropertyName("rank")]
        public int Rank { get; set; }

        [JsonPropertyName("studentId")]
        public string StudentId { get; set; } = string.Empty;

        [JsonPropertyName("studentName")]
        public string StudentName { get; set; } = string.Empty;

        [JsonPropertyName("totalActivities")]
        public int TotalActivities { get; set; }

        [JsonPropertyName("completedActivities")]
        public int CompletedActivities { get; set; }

        [JsonPropertyName("averageScore")]
        public double AverageScore { get; set; }

        [JsonPropertyName("totalPoints")]
        public int TotalPoints { get; set; }

        [JsonPropertyName("levelName")]
        public string LevelName { get; set; } = string.Empty;

        [JsonPropertyName("levelEmoji")]
        public string LevelEmoji { get; set; } = string.Empty;

        [JsonPropertyName("badgeCount")]
        public int BadgeCount { get; set; }

        [JsonPropertyName("badges")]
        public List<BadgeModel> Badges { get; set; } = new();
    }

    // ── HU#16 / HU#17: Puntos e insignias ───────────────────────────────────
    public class BadgeModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("emoji")]
        public string Emoji { get; set; } = string.Empty;

        [JsonPropertyName("earnedAt")]
        public DateTime? EarnedAt { get; set; }
    }

    // ── Resultados por actividad (para el profesor) ──────────────────────────
    public class ActivityResultsResponse
    {
        [JsonPropertyName("activityId")]
        public int ActivityId { get; set; }

        [JsonPropertyName("activityTitle")]
        public string ActivityTitle { get; set; } = string.Empty;

        [JsonPropertyName("totalStudents")]
        public int TotalStudents { get; set; }

        [JsonPropertyName("submitted")]
        public int Submitted { get; set; }

        [JsonPropertyName("results")]
        public List<StudentResult> Results { get; set; } = new();
    }

    public class StudentResult
    {
        [JsonPropertyName("studentId")]
        public string StudentId { get; set; } = string.Empty;

        [JsonPropertyName("studentName")]
        public string StudentName { get; set; } = string.Empty;

        [JsonPropertyName("studentEmail")]
        public string StudentEmail { get; set; } = string.Empty;

        [JsonPropertyName("submittedAt")]
        public DateTime? SubmittedAt { get; set; }

        [JsonPropertyName("score")]
        public int? Score { get; set; }

        [JsonPropertyName("correct")]
        public int Correct { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("questions")]
        public List<QuestionFeedback> Questions { get; set; } = new();
    }
}
