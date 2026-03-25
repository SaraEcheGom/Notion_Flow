namespace NotionFlow.Api.Models
{
    /// <summary>
    /// Representa un curso dentro de una institución.
    /// Cada curso pertenece a una institución específica.
    /// Un curso puede tener múltiples profesores a través de CourseTeacher.
    /// </summary>
    public class Course
    {
        public int Id { get; set; }
        public int InstitutionId { get; set; } // La institución propietaria del curso
        public string Name { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Relaciones
        public Institution? Institution { get; set; }
        public List<CourseTeacher> Teachers { get; set; } = new(); // Múltiples profesores por curso
        public List<CourseStudent> CourseStudents { get; set; } = new();
        public List<Evaluation> Evaluations { get; set; } = new();
        public List<Content> Contents { get; set; } = new();
    }
}