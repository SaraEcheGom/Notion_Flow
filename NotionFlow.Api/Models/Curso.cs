namespace NotionFlow.Api.Models
{
    public class Curso
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Materia { get; set; } = string.Empty;
        public string ProfesorId { get; set; } = string.Empty;
        public string ProfesorNombre { get; set; } = string.Empty;
        public List<CursoEstudiante> CursoEstudiantes { get; set; } = new();
        public List<Evaluacion> Evaluaciones { get; set; } = new();
        public List<Contenido> Contenidos { get; set; } = new();
    }
}