namespace NotionFlow.App.Models.Users
{
    public class Teacher
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public int InstitutionId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public List<Student> Students { get; set; } = new();
    }
}
