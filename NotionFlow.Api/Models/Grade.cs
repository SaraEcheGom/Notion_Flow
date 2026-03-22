namespace NotionFlow.Api.Models
{
    public class Grade
    {
        public int Id { get; set; }
        public int EvaluationId { get; set; }
        public Evaluation? Evaluation { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public double Value { get; set; }
    }
}