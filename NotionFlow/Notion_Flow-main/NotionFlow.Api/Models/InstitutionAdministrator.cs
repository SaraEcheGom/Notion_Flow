namespace NotionFlow.Api.Models
{
    /// <summary>
    /// Representa la relación entre un usuario administrador y una institución.
    /// Un administrador puede gestionar una o más instituciones.
    /// Una institución puede tener uno o más administradores.
    /// </summary>
    public class InstitutionAdministrator
    {
        public int Id { get; set; }
        public int InstitutionId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public bool IsOwner { get; set; } = false; // Indica si es el propietario/fundador de la institución

        // Relaciones
        public Institution? Institution { get; set; }
        public User? User { get; set; }
    }
}
