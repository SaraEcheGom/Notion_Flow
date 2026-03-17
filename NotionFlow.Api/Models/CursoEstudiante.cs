namespace NotionFlow.Api.Models
{
    public class CursoEstudiante
    {
        public int CursoId { get; set; }
        public Curso? Curso { get; set; }
        public string EstudianteId { get; set; } = string.Empty;
    }
}