using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotionFlow.Api.Data;
using NotionFlow.Api.DTOs;
using NotionFlow.Api.Models;
using System.Security.Claims;

namespace NotionFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(
            AppDbContext db,
            UserManager<User> userManager,
            ILogger<CoursesController> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCourse(CreateCourseDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Usuario no autenticado");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound($"Usuario '{userId}' no encontrado");

                if (!user.InstitutionId.HasValue)
                    return Unauthorized("Usuario no pertenece a institución");

                var teacher = await _userManager.FindByIdAsync(dto.TeacherId);
                if (teacher == null)
                    return NotFound($"Profesor '{dto.TeacherId}' no encontrado");

                if (teacher.InstitutionId != user.InstitutionId)
                    return BadRequest("El profesor no pertenece a esta institución");

                var course = new Course
                {
                    Name = dto.Name,
                    Subject = dto.Subject,
                    InstitutionId = user.InstitutionId.Value,
                    Description = dto.Description,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _db.Courses.Add(course);
                await _db.SaveChangesAsync();

                _db.CourseTeachers.Add(new CourseTeacher
                {
                    CourseId = course.Id,
                    TeacherId = dto.TeacherId,
                    IsPrimary = true,
                    AssignedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();

                _logger.LogInformation("Curso creado: {CourseId} '{CourseName}' por {UserId}", course.Id, course.Name, userId);
                return Ok(new { course.Id, course.Name, course.Subject, message = "Curso creado exitosamente" });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException en CreateCourse");
                return StatusCode(500, new { error = "Error al guardar en la base de datos", details = dbEx.InnerException?.Message ?? dbEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción inesperada en CreateCourse");
                return StatusCode(500, new { error = "Error al crear el curso", details = ex.Message });
            }
        }

        [HttpPost("{courseId}/students")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignStudent(int courseId, AssignStudentDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.StudentId))
                    return BadRequest("StudentId es requerido");

                var course = await _db.Courses.FindAsync(courseId);
                if (course == null)
                    return NotFound($"Curso {courseId} no encontrado");

                var student = await _userManager.FindByIdAsync(dto.StudentId);
                if (student == null)
                    return NotFound($"Estudiante {dto.StudentId} no encontrado");

                if (student.InstitutionId != course.InstitutionId)
                    return BadRequest("El estudiante no pertenece a la misma institución que el curso");

                var exists = await _db.CourseStudents
                    .AnyAsync(cs => cs.CourseId == courseId && cs.StudentId == dto.StudentId);
                if (exists)
                    return BadRequest("El estudiante ya está asignado a este curso");

                _db.CourseStudents.Add(new CourseStudent { CourseId = courseId, StudentId = dto.StudentId });
                await _db.SaveChangesAsync();

                _logger.LogInformation("Estudiante {StudentId} asignado al curso {CourseId}", dto.StudentId, courseId);
                return Ok(new { message = "Estudiante asignado exitosamente", courseId, studentId = dto.StudentId, studentName = student.Name });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException en AssignStudent");
                return StatusCode(500, new { error = "Error de base de datos", details = dbEx.InnerException?.Message ?? dbEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción inesperada en AssignStudent");
                return StatusCode(500, new { error = "Error inesperado", details = ex.Message });
            }
        }

        [HttpDelete("{courseId}/students/{studentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveStudent(int courseId, string studentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentId))
                    return BadRequest("StudentId es requerido");

                var course = await _db.Courses.FindAsync(courseId);
                if (course == null)
                    return NotFound($"Curso {courseId} no encontrado");

                var student = await _userManager.FindByIdAsync(studentId);
                if (student == null)
                    return NotFound($"Estudiante {studentId} no encontrado");

                var courseStudent = await _db.CourseStudents
                    .FirstOrDefaultAsync(cs => cs.CourseId == courseId && cs.StudentId == studentId);
                if (courseStudent == null)
                    return NotFound("El estudiante no está asignado a este curso");

                _db.CourseStudents.Remove(courseStudent);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Estudiante {StudentId} removido del curso {CourseId}", studentId, courseId);
                return Ok(new { message = "Estudiante removido exitosamente", courseId, studentId, studentName = student.Name });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DbUpdateException en RemoveStudent");
                return StatusCode(500, new { error = "Error de base de datos", details = dbEx.InnerException?.Message ?? dbEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción inesperada en RemoveStudent");
                return StatusCode(500, new { error = "Error inesperado", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuario no autenticado");

            var user = await _userManager.FindByIdAsync(userId);
            if (user?.InstitutionId == null)
                return Unauthorized("Usuario no pertenece a institución");

            var courses = await _db.Courses
                .AsNoTracking()
                .Where(c => c.InstitutionId == user.InstitutionId)
                .Include(c => c.CourseStudents)
                .Include(c => c.Teachers).ThenInclude(ct => ct.Teacher)
                .ToListAsync();

            var result = courses.Select(c => new CourseResponseDto(
                c.Id,
                c.Name,
                c.Subject,
                c.Teachers.FirstOrDefault(t => t.IsPrimary)?.TeacherId ?? "",
                c.Teachers.FirstOrDefault(t => t.IsPrimary)?.Teacher?.Name ?? "Sin profesor",
                c.CourseStudents.Select(ce => new StudentDto(
                    ce.StudentId,
                    _userManager.Users.FirstOrDefault(u => u.Id == ce.StudentId)?.Name ?? "",
                    _userManager.Users.FirstOrDefault(u => u.Id == ce.StudentId)?.Email ?? ""
                )).ToList()
            ));

            return Ok(result);
        }

        [HttpGet("teacher/{teacherId}")]
        public async Task<IActionResult> CoursesForTeacher(string teacherId)
            => await CoursesForTeacherInternal(teacherId);

        [HttpGet("professor/{teacherId}")]
        public async Task<IActionResult> CoursesForProfessor(string teacherId)
            => await CoursesForTeacherInternal(teacherId);

        private async Task<IActionResult> CoursesForTeacherInternal(string teacherId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuario no autenticado");

            var user = await _userManager.FindByIdAsync(userId);
            if (user?.InstitutionId == null)
                return Unauthorized("Usuario no pertenece a institución");

            var courses = await _db.Courses
                .AsNoTracking()
                .Where(c => c.InstitutionId == user.InstitutionId)
                .Include(c => c.CourseStudents)
                .Include(c => c.Teachers).ThenInclude(ct => ct.Teacher)
                .ToListAsync();

            var coursesByTeacher = courses
                .Where(c => c.Teachers.Any(t => t.TeacherId == teacherId))
                .ToList();

            var result = coursesByTeacher.Select(c => new CourseResponseDto(
                c.Id,
                c.Name,
                c.Subject,
                teacherId,
                _userManager.Users.FirstOrDefault(u => u.Id == teacherId)?.Name ?? "Sin profesor",
                c.CourseStudents.Select(ce => new StudentDto(
                    ce.StudentId,
                    _userManager.Users.FirstOrDefault(u => u.Id == ce.StudentId)?.Name ?? "",
                    _userManager.Users.FirstOrDefault(u => u.Id == ce.StudentId)?.Email ?? ""
                )).ToList()
            ));

            return Ok(result);
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> CoursesForStudent(string studentId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuario no autenticado");

            var user = await _userManager.FindByIdAsync(userId);
            if (user?.InstitutionId == null)
                return Unauthorized("Usuario no pertenece a institución");

            var courses = await _db.CourseStudents
                .AsNoTracking()
                .Where(ce => ce.StudentId == studentId)
                .Include(ce => ce.Course).ThenInclude(c => c!.Teachers).ThenInclude(ct => ct.Teacher)
                .Select(ce => ce.Course)
                .Where(c => c!.InstitutionId == user.InstitutionId)
                .ToListAsync();

            var result = courses.Select(c => new CourseResponseDto(
                c!.Id,
                c.Name,
                c.Subject,
                c.Teachers.FirstOrDefault(t => t.IsPrimary)?.TeacherId ?? "",
                c.Teachers.FirstOrDefault(t => t.IsPrimary)?.Teacher?.Name ?? "Sin profesor",
                new List<StudentDto>()
            ));

            return Ok(result);
        }

        [HttpGet("{courseId}/evaluations")]
        [Authorize]
        public async Task<IActionResult> GetEvaluations(int courseId)
        {
            var evaluations = await _db.Evaluations
                .AsNoTracking()
                .Where(e => e.CourseId == courseId)
                .ToListAsync();
            return Ok(evaluations);
        }

        [HttpPost("{courseId}/evaluations")]
        [Authorize(Roles = "Professor")]
        public async Task<IActionResult> CreateEvaluation(int courseId, CreateEvaluationDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuario no autenticado");

            var isTeacherOfCourse = await _db.CourseTeachers
                .AnyAsync(ct => ct.CourseId == courseId && ct.TeacherId == userId);
            if (!isTeacherOfCourse)
                return StatusCode(403, new { error = "No eres profesor de este curso" });

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

            _logger.LogInformation("Evaluación creada en curso {CourseId} por profesor {UserId}", courseId, userId);
            return Ok(evaluation);
        }

        [HttpPost("evaluations/{evaluationId}/grades")]
        [Authorize(Roles = "Professor")]
        public async Task<IActionResult> SaveGrade(int evaluationId, SaveGradeDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuario no autenticado");

            var evaluation = await _db.Evaluations.FindAsync(evaluationId);
            if (evaluation == null)
                return NotFound(new { error = "Evaluación no encontrada" });

            // El profesor debe ser docente del curso de esta evaluación
            var isTeacherOfCourse = await _db.CourseTeachers
                .AnyAsync(ct => ct.CourseId == evaluation.CourseId && ct.TeacherId == userId);
            if (!isTeacherOfCourse)
                return StatusCode(403, new { error = "No eres profesor del curso de esta evaluación" });

            // El estudiante debe estar matriculado en el curso
            var isStudentEnrolled = await _db.CourseStudents
                .AnyAsync(cs => cs.CourseId == evaluation.CourseId && cs.StudentId == dto.StudentId);
            if (!isStudentEnrolled)
                return BadRequest(new { error = "El estudiante no está matriculado en este curso" });

            var grade = await _db.Grades
                .FirstOrDefaultAsync(n => n.EvaluationId == evaluationId && n.StudentId == dto.StudentId);

            if (grade == null)
            {
                grade = new Grade { EvaluationId = evaluationId, StudentId = dto.StudentId, Value = dto.Value };
                _db.Grades.Add(grade);
            }
            else
            {
                grade.Value = dto.Value;
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("Nota guardada para estudiante {StudentId} en evaluación {EvaluationId}", dto.StudentId, evaluationId);
            return Ok(grade);
        }

        [HttpGet("{courseId}/contents")]
        public async Task<IActionResult> GetContents(int courseId)
        {
            var contents = await _db.Contents
                .AsNoTracking()
                .Where(c => c.CourseId == courseId)
                .OrderByDescending(c => c.PublicationDate)
                .ToListAsync();
            return Ok(contents);
        }

        [HttpPost("{courseId}/contents")]
        [Authorize(Roles = "Professor")]
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
