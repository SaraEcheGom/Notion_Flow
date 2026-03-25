using Microsoft.AspNetCore.Identity;

namespace NotionFlow.Api.Models
{
    /// <summary>
    /// Representa un usuario del sistema (profesor, estudiante, administrador).
    /// Cada usuario pertenece a una institución específica.
    /// Los administradores pueden gestionar múltiples instituciones a través de InstitutionAdministrator.
    /// </summary>
    public class User : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? InstitutionId { get; set; } // La institución principal del usuario
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Relaciones
        public Institution? Institution { get; set; }
        public List<InstitutionAdministrator> AdministratorOf { get; set; } = new(); // Instituciones que administra
        public List<CourseTeacher> TaughtCourses { get; set; } = new(); // Cursos que enseña (solo para profesores)
    }
}