namespace NotionFlow.Api.DTOs
{
    public record CreateCourseDto(
        string Name,
        string Subject,
        string Description,
        string TeacherId
    );

    public record AssignStudentDto(
        [System.ComponentModel.DataAnnotations.Required] 
        string StudentId
    );

    public record CourseResponseDto(
        int Id,
        string Name,
        string Subject,
        string TeacherId,
        string TeacherName,
        List<StudentDto> Students
    );

    public record StudentDto(
        string Id,
        string Name,
        string Email
    );

    public record CreateEvaluationDto(
        string Title,
        string Description,
        DateTime Date,
        double PercentageValue
    );

    public record SaveGradeDto(
        string StudentId,
        double Value
    );

    public record PublishContentDto(
        string Title,
        string Description,
        string Type,
        string Url
    );
}