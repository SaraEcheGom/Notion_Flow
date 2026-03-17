namespace NotionFlow.App.Models
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
    }

    public class CursoResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Materia { get; set; } = string.Empty;
        public string ProfesorNombre { get; set; } = string.Empty;
        public List<EstudianteItem> Estudiantes { get; set; } = new();
    }

    public class EstudianteItem
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class Evaluacion
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public double PorcentajeValor { get; set; }
    }

    public class Contenido
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}