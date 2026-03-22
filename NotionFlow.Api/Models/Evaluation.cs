namespace NotionFlow.Api.Models
{
    public class Evaluation
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course? Course { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public double PercentageValue { get; set; }
        public List<Grade> Grades { get; set; } = new();
    }
}