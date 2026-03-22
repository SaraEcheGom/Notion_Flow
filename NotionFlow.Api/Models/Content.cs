namespace NotionFlow.Api.Models
{
    public class Contenido
    {
        public int Id { get; set; }
        public int CursoId { get; set; }
        public Curso? Curso { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;
    }
}