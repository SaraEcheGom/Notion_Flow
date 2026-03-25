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
            try
            {
                Console.WriteLine($"\n📥 [CoursesController] CreateCourse called");
                Console.WriteLine($"  Name: '{dto.Name}'");
                Console.WriteLine($"  Subject: '{dto.Subject}'");
                Console.WriteLine($"  Description: '{dto.Description}'");
                Console.WriteLine($"  TeacherId: '{dto.TeacherId}'");

                // Step 1: Get current user
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"  Current UserId: '{userId}'");

                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("✗ [CoursesController] User not authenticated");
                    return Unauthorized("User not authenticated");
                }

                // Step 2: Find user in database
                Console.WriteLine($"  Looking up user '{userId}' in database...");
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    Console.WriteLine($"✗ [CoursesController] User '{userId}' not found in database");
                    return NotFound($"Usuario '{userId}' no encontrado en la base de datos");
                }

                Console.WriteLine($"  User found: {user.Name}, InstitutionId: {user.InstitutionId}");

                if (!user.InstitutionId.HasValue)
                {
                    Console.WriteLine("✗ [CoursesController] User has no InstitutionId");
                    return Unauthorized("Usuario no pertenece a institución");
                }

                // Step 3: Find teacher
                Console.WriteLine($"  Looking up teacher '{dto.TeacherId}' in database...");
                var teacher = await _userManager.FindByIdAsync(dto.TeacherId);

                if (teacher == null)
                {
                    Console.WriteLine($"✗ [CoursesController] Teacher '{dto.TeacherId}' not found");
                    return NotFound($"Profesor '{dto.TeacherId}' no encontrado");
                }

                Console.WriteLine($"  Teacher found: {teacher.Name}, InstitutionId: {teacher.InstitutionId}");

                // Step 4: Validate teacher belongs to same institution
                if (teacher.InstitutionId != user.InstitutionId)
                {
                    Console.WriteLine($"✗ [CoursesController] Teacher InstitutionId ({teacher.InstitutionId}) != User InstitutionId ({user.InstitutionId})");
                    return BadRequest($"Profesor no pertenece a esta institución. Teacher: {teacher.InstitutionId}, User: {user.InstitutionId}");
                }

                // Step 5: Create course
                Console.WriteLine($"  Creating course in database...");
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
                Console.WriteLine($"✓ [CoursesController] Course created with ID: {course.Id}");

                // Step 6: Assign teacher to course
                Console.WriteLine($"  Assigning teacher to course...");
                var courseTeacher = new CourseTeacher
                {
                    CourseId = course.Id,
                    TeacherId = dto.TeacherId,
                    IsPrimary = true,
                    AssignedAt = DateTime.UtcNow
                };
                _db.CourseTeachers.Add(courseTeacher);
                await _db.SaveChangesAsync();
                Console.WriteLine($"✓ [CoursesController] Teacher assigned to course");

                Console.WriteLine($"✓ [CoursesController] Course created successfully!");
                return Ok(new { course.Id, course.Name, course.Subject, message = "Curso creado exitosamente" });
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"\n✗ [CoursesController] DbUpdateException: {dbEx.Message}");
                Console.WriteLine($"  Inner Exception: {dbEx.InnerException?.Message}");
                Console.WriteLine($"  StackTrace: {dbEx.StackTrace}");
                return StatusCode(500, new { 
                    error = "Error al guardar en la base de datos", 
                    details = dbEx.InnerException?.Message ?? dbEx.Message 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ [CoursesController] Unexpected Exception: {ex.GetType().Name}");
                Console.WriteLine($"  Message: {ex.Message}");
                Console.WriteLine($"  StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"  Inner Exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { 
                    error = "Error al crear el curso", 
                    details = ex.Message,
                    exceptionType = ex.GetType().Name
                });
            }
        }

        [HttpPost("{courseId}/students")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignStudent(int courseId, AssignStudentDto dto)
        {
            try
            {
                Console.WriteLine($"\n📥 [CoursesController] AssignStudent called");
                Console.WriteLine($"  CourseId: {courseId}");
                Console.WriteLine($"  StudentId: {dto?.StudentId}");
                Console.WriteLine($"  DTO is null: {dto == null}");

                // Validar DTO
                if (dto == null || string.IsNullOrWhiteSpace(dto.StudentId))
                {
                    Console.WriteLine("✗ [CoursesController] Invalid DTO or StudentId is null/empty");
                    return BadRequest("StudentId is required");
                }

                // Validar que el curso existe
                var course = await _db.Courses.FindAsync(courseId);
                if (course == null)
                {
                    Console.WriteLine($"✗ [CoursesController] Course {courseId} not found");
                    return NotFound($"Course {courseId} not found");
                }
                Console.WriteLine($"✓ [CoursesController] Course found: {course.Name}");

                // Validar que el estudiante existe
                var student = await _userManager.FindByIdAsync(dto.StudentId);
                if (student == null)
                {
                    Console.WriteLine($"✗ [CoursesController] Student {dto.StudentId} not found");
                    return NotFound($"Student {dto.StudentId} not found");
                }
                Console.WriteLine($"✓ [CoursesController] Student found: {student.Name}");

                // Validar que el estudiante pertenece a la misma institución
                if (student.InstitutionId != course.InstitutionId)
                {
                    Console.WriteLine($"✗ [CoursesController] Institution mismatch. Student institution: {student.InstitutionId}, Course institution: {course.InstitutionId}");
                    return BadRequest("Student does not belong to the same institution as the course");
                }
                Console.WriteLine($"✓ [CoursesController] Institution match verified (Institution: {course.InstitutionId})");

                // Verificar si ya está asignado
                var exists = await _db.CourseStudents
                    .AnyAsync(cs => cs.CourseId == courseId && cs.StudentId == dto.StudentId);

                if (exists)
                {
                    Console.WriteLine($"⚠️ [CoursesController] Student {dto.StudentId} is already assigned to course {courseId}");
                    return BadRequest("Student is already assigned to this course");
                }

                // Asignar estudiante
                _db.CourseStudents.Add(new CourseStudent
                {
                    CourseId = courseId,
                    StudentId = dto.StudentId
                });

                await _db.SaveChangesAsync();
                Console.WriteLine($"✓ [CoursesController] Student {student.Name} assigned to course {course.Name}");
                return Ok(new { message = "Student assigned successfully", courseId, studentId = dto.StudentId, studentName = student.Name });
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"✗ [CoursesController] DbUpdateException: {dbEx.Message}");
                Console.WriteLine($"  Inner: {dbEx.InnerException?.Message}");
                return StatusCode(500, new { error = "Database error", details = dbEx.InnerException?.Message ?? dbEx.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ [CoursesController] Unexpected Exception: {ex.GetType().Name}");
                Console.WriteLine($"  Message: {ex.Message}");
                return StatusCode(500, new { error = "Unexpected error", details = ex.Message });
            }
        }

        [HttpDelete("{courseId}/students/{studentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveStudent(int courseId, string studentId)
        {
            try
            {
                Console.WriteLine($"\n📥 [CoursesController] RemoveStudent called");
                Console.WriteLine($"  CourseId: {courseId}");
                Console.WriteLine($"  StudentId: {studentId}");

                // Validar StudentId
                if (string.IsNullOrWhiteSpace(studentId))
                {
                    Console.WriteLine("✗ [CoursesController] StudentId is null/empty");
                    return BadRequest("StudentId is required");
                }

                // Validar que el curso existe
                var course = await _db.Courses.FindAsync(courseId);
                if (course == null)
                {
                    Console.WriteLine($"✗ [CoursesController] Course {courseId} not found");
                    return NotFound($"Course {courseId} not found");
                }
                Console.WriteLine($"✓ [CoursesController] Course found: {course.Name}");

                // Validar que el estudiante existe
                var student = await _userManager.FindByIdAsync(studentId);
                if (student == null)
                {
                    Console.WriteLine($"✗ [CoursesController] Student {studentId} not found");
                    return NotFound($"Student {studentId} not found");
                }
                Console.WriteLine($"✓ [CoursesController] Student found: {student.Name}");

                // Buscar la asignación del estudiante al curso
                var courseStudent = await _db.CourseStudents
                    .FirstOrDefaultAsync(cs => cs.CourseId == courseId && cs.StudentId == studentId);

                if (courseStudent == null)
                {
                    Console.WriteLine($"⚠️ [CoursesController] Student {studentId} is not assigned to course {courseId}");
                    return NotFound("Student is not assigned to this course");
                }
                Console.WriteLine($"✓ [CoursesController] CourseStudent relationship found");

                // Remover la asignación
                _db.CourseStudents.Remove(courseStudent);
                await _db.SaveChangesAsync();

                Console.WriteLine($"✓ [CoursesController] Student {student.Name} removed from course {course.Name}");
                return Ok(new { message = "Student removed successfully", courseId, studentId, studentName = student.Name });
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"✗ [CoursesController] DbUpdateException: {dbEx.Message}");
                Console.WriteLine($"  Inner: {dbEx.InnerException?.Message}");
                return StatusCode(500, new { error = "Database error", details = dbEx.InnerException?.Message ?? dbEx.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ [CoursesController] Unexpected Exception: {ex.GetType().Name}");
                Console.WriteLine($"  Message: {ex.Message}");
                return StatusCode(500, new { error = "Unexpected error", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var user = await _userManager.FindByIdAsync(userId);

            if (user?.InstitutionId == null)
                return Unauthorized("Usuario no pertenece a institución");

            var courses = await _db.Courses
                .Where(c => c.InstitutionId == user.InstitutionId)
                .Include(c => c.CourseStudents)
                .Include(c => c.Teachers)
                    .ThenInclude(ct => ct.Teacher)
                .ToListAsync();

            var result = courses.Select(c => new CourseResponseDto(
                c.Id,
                c.Name,
                c.Subject,
                c.Teachers.FirstOrDefault(t => t.IsPrimary)?.TeacherId ?? "",
                c.Teachers.FirstOrDefault(t => t.IsPrimary)?.Teacher?.Name ?? "No teacher",
                c.CourseStudents.Select(ce => new StudentDto(
                    ce.StudentId,
                    _userManager.Users.FirstOrDefault(u => u.Id == ce.StudentId)?.Name ?? "",
                    _userManager.Users.FirstOrDefault(u => u.Id == ce.StudentId)?.Email ?? ""
                )).ToList()
            ));

            return Ok(result);
        }

        [HttpGet("teacher/{teacherId}")]
        [Authorize(Roles = "Admin,Professor")]
        public async Task<IActionResult> CoursesForTeacher(string teacherId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var user = await _userManager.FindByIdAsync(userId);

            if (user?.InstitutionId == null)
                return Unauthorized("Usuario no pertenece a institución");

            var courses = await _db.Courses
                .Where(c => c.InstitutionId == user.InstitutionId)
                .Include(c => c.CourseStudents)
                .Include(c => c.Teachers)
                    .ThenInclude(ct => ct.Teacher)
                .ToListAsync();

            // Filtrar por profesor
            var coursesByTeacher = courses
                .Where(c => c.Teachers.Any(t => t.TeacherId == teacherId))
                .ToList();

            var result = coursesByTeacher.Select(c => new CourseResponseDto(
                c.Id,
                c.Name,
                c.Subject,
                teacherId,
                _userManager.Users.FirstOrDefault(u => u.Id == teacherId)?.Name ?? "No teacher",
                c.CourseStudents.Select(ce => new StudentDto(
                    ce.StudentId,
                    _userManager.Users.FirstOrDefault(u => u.Id == ce.StudentId)?.Name ?? "",
                    _userManager.Users.FirstOrDefault(u => u.Id == ce.StudentId)?.Email ?? ""
                )).ToList()
            ));

            return Ok(result);
        }

        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> CoursesForStudent(string studentId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var user = await _userManager.FindByIdAsync(userId);

            if (user?.InstitutionId == null)
                return Unauthorized("Usuario no pertenece a institución");

            var courses = await _db.CourseStudents
                .Where(ce => ce.StudentId == studentId)
                .Include(ce => ce.Course)
                    .ThenInclude(c => c.Teachers)
                        .ThenInclude(ct => ct.Teacher)
                .Select(ce => ce.Course)
                .Where(c => c.InstitutionId == user.InstitutionId)
                .ToListAsync();

            var result = courses.Select(c => new CourseResponseDto(
                c!.Id,
                c.Name,
                c.Subject,
                c.Teachers.FirstOrDefault(t => t.IsPrimary)?.TeacherId ?? "",
                c.Teachers.FirstOrDefault(t => t.IsPrimary)?.Teacher?.Name ?? "No teacher",
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