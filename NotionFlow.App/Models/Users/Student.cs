namespace NotionFlow.App.Models.Users
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string ProfessorEmail { get; set; } = string.Empty;
    }
}
