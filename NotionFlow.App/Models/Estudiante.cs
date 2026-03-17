namespace NotionFlow.App.Models
{
    public class Estudiante
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Grado { get; set; } = string.Empty;
        public string ProfesorCorreo { get; set; } = string.Empty;
    }
}