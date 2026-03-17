namespace NotionFlow.Api.Models
{
    public class Evaluacion
    {
        public int Id { get; set; }
        public int CursoId { get; set; }
        public Curso? Curso { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public double PorcentajeValor { get; set; }
        public List<Nota> Notas { get; set; } = new();
    }
}