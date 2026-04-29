namespace NotionFlow.Api.Models
{
    /// <summary>
    /// Representa la relación entre un profesor y un curso.
    /// Permite que un profesor enseñe múltiples cursos y un curso tenga múltiples profesores.
    /// </summary>
    public class CourseTeacher
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string TeacherId { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public bool IsPrimary { get; set; } = false; // Indica si es el profesor principal del curso

        // Relaciones
        public Course? Course { get; set; }
        public User? Teacher { get; set; }
    }
}
