namespace NotionFlow.App.Models
{
    public class Profesor
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Materia { get; set; } = string.Empty;
        public List<Estudiante> Estudiantes { get; set; } = new();
    }
}