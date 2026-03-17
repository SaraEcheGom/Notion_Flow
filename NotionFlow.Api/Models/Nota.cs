namespace NotionFlow.Api.Models
{
    public class Nota
    {
        public int Id { get; set; }
        public int EvaluacionId { get; set; }
        public Evaluacion? Evaluacion { get; set; }
        public string EstudianteId { get; set; } = string.Empty;
        public double Valor { get; set; }
    }
}