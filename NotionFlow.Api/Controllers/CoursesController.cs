using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotionFlow.Api.Data;
using NotionFlow.Api.DTOs;
using NotionFlow.Api.Models;

namespace NotionFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<User> _userManager;

        public CoursesController(AppDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCourse(CreateCourseDto dto)
        {
            var teacher = await _userManager.FindByIdAsync(dto.TeacherId);
            var course = new Course
            {
                Name = dto.Name,
                Subject = dto.Subject,
                TeacherId = dto.TeacherId,
                TeacherName = teacher?.Name ?? string.Empty
            };
            _db.Courses.Add(course);
            await _db.SaveChangesAsync();
            return Ok(course);
        }

        [HttpPost("{courseId}/students")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignStudent(int courseId, AssignStudentDto dto)
        {
            var exists = await _db.CourseStudents
                .AnyAsync(ce => ce.CourseId == courseId && ce.StudentId == dto.StudentId);

            if (exists) return BadRequest("Student is already in this course");

            _db.CourseStudents.Add(new CourseStudent
            {
                CourseId = courseId,
                StudentId = dto.StudentId
            });
            await _db.SaveChangesAsync();
            return Ok("Student assigned");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var courses = await _db.Courses
                .Include(c => c.CourseStudents)
                .ToListAsync();

            var users = await _userManager.Users.ToListAsync();

            var result = courses.Select(c => new CourseResponseDto(
                c.Id,
                c.Name,
                c.Subject,
                c.TeacherId,
                users.FirstOrDefault(u => u.Id == c.TeacherId)?.Name ?? "No teacher",
                c.CourseStudents.Select(ce => new StudentDto(
                    ce.StudentId,
                    users.FirstOrDefault(u => u.Id == ce.StudentId)?.Name ?? "",
                    users.FirstOrDefault(u => u.Id == ce.StudentId)?.Email ?? ""
                )).ToList()
            ));

            return Ok(result);
        }

        [HttpGet("teacher/{teacherId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CoursesForTeacher(string teacherId)
        {
            var courses = await _db.Courses
                .Where(c => c.TeacherId == teacherId)
                .Include(c => c.CourseStudents)
                .ToListAsync();

            var users = await _userManager.Users.ToListAsync();

            var result = courses.Select(c => new CourseResponseDto(
                c.Id,
                c.Name,
                c.Subject,
                c.TeacherId,
                users.FirstOrDefault(u => u.Id == c.TeacherId)?.Name ?? "No teacher",
                c.CourseStudents.Select(ce => new StudentDto(
                    ce.StudentId,
                    users.FirstOrDefault(u => u.Id == ce.StudentId)?.Name ?? "",
                    users.FirstOrDefault(u => u.Id == ce.StudentId)?.Email ?? ""
                )).ToList()
            ));

            return Ok(result);
        }

        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> CoursesForStudent(string studentId)
        {
            var courses = await _db.CourseStudents
                .Where(ce => ce.StudentId == studentId)
                .Include(ce => ce.Course)
                .Select(ce => ce.Course)
                .ToListAsync();

            var users = await _userManager.Users.ToListAsync();

            var result = courses.Select(c => new CourseResponseDto(
                c!.Id,
                c.Name,
                c.Subject,
                c.TeacherId,
                users.FirstOrDefault(u => u.Id == c.TeacherId)?.Name ?? "No teacher",
                new List<StudentDto>()
            ));

            return Ok(result);
        }

        [HttpGet("{courseId}/evaluations")]
        public async Task<IActionResult> GetEvaluations(int courseId)
        {
            var evaluations = await _db.Evaluations
                .Where(e => e.CourseId == courseId)
                .ToListAsync();
            return Ok(evaluations);
        }

        [HttpPost("{courseId}/evaluations")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateEvaluation(int courseId, CreateEvaluationDto dto)
        {
            var evaluation = new Evaluation
            {
                CourseId = courseId,
                Title = dto.Title,
                Description = dto.Description,
                Date = dto.Date,
                PercentageValue = dto.PercentageValue
            };
            _db.Evaluations.Add(evaluation);
            await _db.SaveChangesAsync();
            return Ok(evaluation);
        }

        [HttpPost("evaluations/{evaluationId}/grades")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> SaveGrade(int evaluationId, SaveGradeDto dto)
        {
            var grade = await _db.Grades
                .FirstOrDefaultAsync(n => n.EvaluationId == evaluationId
                    && n.StudentId == dto.StudentId);

            if (grade == null)
            {
                grade = new Grade
                {
                    EvaluationId = evaluationId,
                    StudentId = dto.StudentId,
                    Value = dto.Value
                };
                _db.Grades.Add(grade);
            }
            else
            {
                grade.Value = dto.Value;
            }

            await _db.SaveChangesAsync();
            return Ok(grade);
        }

        [HttpGet("{courseId}/contents")]
        public async Task<IActionResult> GetContents(int courseId)
        {
            var contents = await _db.Contents
                .Where(c => c.CourseId == courseId)
                .OrderByDescending(c => c.PublicationDate)
                .ToListAsync();
            return Ok(contents);
        }

        [HttpPost("{courseId}/contents")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> PublishContent(int courseId, PublishContentDto dto)
        {
            var content = new Content
            {
                CourseId = courseId,
                Title = dto.Title,
                Description = dto.Description,
                Type = dto.Type,
                Url = dto.Url,
                PublicationDate = DateTime.UtcNow
            };
            _db.Contents.Add(content);
            await _db.SaveChangesAsync();
            return Ok(content);
        }
    }
}