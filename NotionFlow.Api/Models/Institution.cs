namespace NotionFlow.Api.Models
{
    /// <summary>
    /// Representa una institución educativa (colegio, universidad, etc.)
    /// Esta es la entidad raíz para modelar un sistema multi-institución.
    /// </summary>
    public class Institution
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string RegistrationCode { get; set; } = string.Empty; // Código único de registro de la institución
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Relaciones
        public List<User> Users { get; set; } = new();
        public List<Course> Courses { get; set; } = new();
        public List<InstitutionAdministrator> Administrators { get; set; } = new();
    }
}
