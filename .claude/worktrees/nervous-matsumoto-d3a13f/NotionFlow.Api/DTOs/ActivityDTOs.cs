namespace NotionFlow.Api.DTOs
{
    // ── HU #5: Crear actividad tipo cuestionario ─────────────────────────────
    public record CreateActivityDto(
        string Title,
        string? Description,
        DateTime? DueDate,
        double PercentageValue,
        List<CreateQuestionDto> Questions
    );

    public record CreateQuestionDto(
        string Text,
        string QuestionType,
        List<CreateOptionDto> Options
    );

    public record CreateOptionDto(
        string Text,
        bool IsCorrect
    );

    // ── HU #6: Editar actividad ──────────────────────────────────────────────
    public record UpdateActivityDto(
        string Title,
        string? Description,
        DateTime? DueDate,
        double PercentageValue,
        List<CreateQuestionDto> Questions
    );

    // ── HU #8: Asignar actividad ─────────────────────────────────────────────
    public record AssignActivityDto(
        List<string> StudentIds
    );

    // ── Submit actividad (estudiante responde) ───────────────────────────────
    public record SubmitActivityDto(
        List<AnswerDto> Answers
    );

    public record AnswerDto(
        int QuestionId,
        List<int> SelectedOptionIds,
        string? TextAnswer
    );

    // ── Respuesta ────────────────────────────────────────────────────────────
    public record ActivityResponseDto(
        int Id,
        int CourseId,
        string Title,
        string? Description,
        DateTime? DueDate,
        double PercentageValue,
        DateTime CreatedAt,
        List<QuestionResponseDto> Questions,
        List<AssignmentResponseDto> Assignments
    );

    public record QuestionResponseDto(
        int Id,
        string Text,
        string QuestionType,
        List<OptionResponseDto> Options
    );

    public record OptionResponseDto(
        int Id,
        string Text,
        bool IsCorrect
    );

    public record AssignmentResponseDto(
        int Id,
        string StudentId,
        DateTime AssignedAt
    );

    // ── Resultado de submit (feedback al estudiante) ─────────────────────────
    public record SubmitResultDto(
        string Message,
        int? Score,
        int Correct,
        int Total,
        List<QuestionFeedbackDto> Answers
    );

    public record QuestionFeedbackDto(
        int QuestionId,
        string QuestionText,
        string QuestionType,
        bool IsCorrect,
        List<int> SelectedOptionIds,
        List<int> CorrectOptionIds,
        List<OptionResponseDto> Options,
        string? TextAnswer
    );
}
