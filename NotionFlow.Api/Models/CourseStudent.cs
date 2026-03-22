namespace NotionFlow.Api.Models
{
    public class CourseStudent
    {
        public int CourseId { get; set; }
        public Course? Course { get; set; }
        public string StudentId { get; set; } = string.Empty;
    }
}