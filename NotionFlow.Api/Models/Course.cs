namespace NotionFlow.Api.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string TeacherId { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public List<CourseStudent> CourseStudents { get; set; } = new();
        public List<Evaluation> Evaluations { get; set; } = new();
        public List<Content> Contents { get; set; } = new();
    }
}