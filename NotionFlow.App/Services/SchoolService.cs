using NotionFlow.App.Models;
using NotionFlow.App.Models.Users;

namespace NotionFlow.App.Services
{
    public class SchoolService
    {
        public static List<Professor> Professors = new();

        public static List<Student> Students = new();

        public Professor CreateProfessor(string name, string email, string subject)
        {
            var professor = new Professor
            {
                Id = Professors.Count + 1,
                Name = name,
                Email = email,
                Subject = subject
            };

            Professors.Add(professor);

            return professor;
        }

        public Student CreateStudent(string name, string grade, string professorEmail)
        {
            var student = new Student
            {
                Id = Students.Count + 1,
                Name = name,
                Grade = grade,
                ProfessorEmail = professorEmail
            };

            Students.Add(student);

            var professor = Professors.FirstOrDefault(p => p.Email == professorEmail);

            if (professor != null)
            {
                professor.Students.Add(student);
            }

            return student;
        }

        public List<Student> GetStudentsForProfessor(string professorEmail)
        {
            return Students
                .Where(e => e.ProfessorEmail == professorEmail)
                .ToList();
        }
    }
}