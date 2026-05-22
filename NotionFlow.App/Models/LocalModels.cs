using System.ComponentModel.DataAnnotations;

namespace NotionFlow.App.Models;

public class UserLocal
{
    [Key]
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int InstitutionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class InstitutionLocal
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string RegistrationCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class CourseLocal
{
    [Key]
    public int Id { get; set; }
    public int InstitutionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public List<ContentLocal> Contents { get; set; } = new();
    public List<EvaluationLocal> Evaluations { get; set; } = new();
}

public class ContentLocal
{
    [Key]
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime PublicationDate { get; set; }
}

public class EvaluationLocal
{
    [Key]
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public double PercentageValue { get; set; }
    public List<GradeLocal> Grades { get; set; } = new();
}

public class GradeLocal
{
    [Key]
    public int Id { get; set; }
    public int EvaluationId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public double Value { get; set; }
}

public class CourseTeacherLocal
{
    [Key]
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string TeacherId { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public DateTime AssignedAt { get; set; }
}

public class CourseStudentLocal
{
    [Key]
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string StudentId { get; set; } = string.Empty;
}

