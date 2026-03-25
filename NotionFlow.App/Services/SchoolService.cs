using NotionFlow.App.Models;
using NotionFlow.App.Models.Users;

namespace NotionFlow.App.Services
{
    public class SchoolService
    {
        public static List<Teacher> Teachers = new();

        public static List<Student> Students = new();

        public Teacher CreateTeacher(string name, string email, string subject)
        {
            var teacher = new Teacher
            {
                Name = name,
                Email = email,
                Subject = subject
            };

            Teachers.Add(teacher);

            return teacher;
        }

        public Student CreateStudent(string name, string grade, string teacherEmail)
        {
            var student = new Student
            {
                Id = Students.Count + 1,
                Name = name,
                Grade = grade,
                ProfessorEmail = teacherEmail
            };

            Students.Add(student);

            var teacher = Teachers.FirstOrDefault(p => p.Email == teacherEmail);

            if (teacher != null)
            {
                teacher.Students.Add(student);
            }

            return student;
        }

        public List<Student> GetStudentsForTeacher(string teacherEmail)
        {
            return Students
                .Where(e => e.ProfessorEmail == teacherEmail)
                .ToList();
        }
    }
}