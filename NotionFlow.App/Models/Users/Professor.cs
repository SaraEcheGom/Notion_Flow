namespace NotionFlow.App.Models.Users
{
    public class Professor
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public List<Student> Students { get; set; } = new();
    }
}
