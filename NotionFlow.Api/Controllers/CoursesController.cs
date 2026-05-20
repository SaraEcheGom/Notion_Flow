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
        [Authorize]
        public async Task<IActionResult> CoursesForTeacher(string teacherId)
        {
            return await CoursesForTeacherInternal(teacherId);
        }

        [HttpGet("professor/{teacherId}")]
        [Authorize]
        public async Task<IActionResult> CoursesForProfessor(string teacherId)
        {
            return await CoursesForTeacherInternal(teacherId);
        }

        private async Task<IActionResult> CoursesForTeacherInternal(string teacherId)
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
        [Authorize]
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
                    .ThenInclude(c => c!.Teachers)
                        .ThenInclude(ct => ct.Teacher)
                .Select(ce => ce.Course)
                .Where(c => c!.InstitutionId == user.InstitutionId)
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
        [Authorize(Roles = "Professor")]
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
        [Authorize(Roles = "Professor")]
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

        // ── HU#13 & HU#14: Progreso individual de un estudiante ─────────────
        [HttpGet("{courseId}/progress/{studentId}")]
        public async Task<IActionResult> GetStudentProgress(int courseId, string studentId)
        {
            var student = await _userManager.FindByIdAsync(studentId);
            if (student == null) return NotFound("Estudiante no encontrado");

            var activities = await _db.Activities
                .Include(a => a.Assignments)
                .Where(a => a.CourseId == courseId)
                .ToListAsync();

            var assignedActivities = activities
                .Where(a => a.Assignments.Any(asgn => asgn.StudentId == studentId))
                .ToList();

            var activityDetails = assignedActivities.Select(a =>
            {
                var asgn = a.Assignments.First(x => x.StudentId == studentId);
                return new
                {
                    activityId = a.Id,
                    activityTitle = a.Title,
                    score = asgn.Score,
                    submittedAt = asgn.SubmittedAt,
                    completed = asgn.SubmittedAt.HasValue,
                    percentageValue = a.PercentageValue
                };
            }).ToList();

            var completed = activityDetails.Where(d => d.completed).ToList();

            // ── Sistema de Puntos mejorado (HU#16) ──────────────────────────
            // Puntos = score ponderado por el porcentaje de la actividad + bonificaciones
            double weightedPoints = 0;
            foreach (var act in completed)
            {
                double basePoints = (act.score ?? 0) * (act.percentageValue / 100.0) * 10;
                // Bonus por puntaje perfecto (+20%)
                if (act.score == 100) basePoints *= 1.20;
                // Bonus por entrega a tiempo (si se entregó, se asume a tiempo en esta versión)
                else if (act.score >= 80) basePoints *= 1.10;
                weightedPoints += basePoints;
            }
            var totalPoints = (int)Math.Round(weightedPoints);

            var averageScore = completed.Count > 0
                ? completed.Average(d => (double)(d.score ?? 0))
                : 0;

            // ── Nivel del estudiante según puntos ───────────────────────────
            var (levelName, levelEmoji, nextLevelPoints) = CalculateLevel(totalPoints);

            // ── Sistema de Insignias completo (HU#17) ───────────────────────
            var badges = CalculateBadges(completed.Select(d => new CompletedActivity
            {
                Score = d.score,
                SubmittedAt = d.submittedAt,
                ActivityTitle = d.activityTitle
            }).ToList(), totalPoints, averageScore, assignedActivities.Count);

            // ── Racha de actividades (streak) ────────────────────────────────
            int streak = CalculateStreak(completed
                .Where(d => d.submittedAt.HasValue)
                .OrderByDescending(d => d.submittedAt)
                .Select(d => d.submittedAt!.Value)
                .ToList());

            return Ok(new
            {
                studentId,
                studentName = student.Name,
                totalActivities = assignedActivities.Count,
                completedActivities = completed.Count,
                averageScore = Math.Round(averageScore, 1),
                totalPoints,
                levelName,
                levelEmoji,
                nextLevelPoints,
                streak,
                activityDetails,
                badges
            });
        }

        // ── Helpers: Sistema de Puntos / Niveles / Insignias ─────────────────

        private record CompletedActivity
        {
            public int? Score { get; init; }
            public DateTime? SubmittedAt { get; init; }
            public string ActivityTitle { get; init; } = "";
        }

        private static (string Name, string Emoji, int NextLevel) CalculateLevel(int points) => points switch
        {
            < 50   => ("Principiante", "🌱", 50),
            < 150  => ("Aprendiz",     "📚", 150),
            < 300  => ("Intermedio",   "⚡", 300),
            < 500  => ("Avanzado",     "🔥", 500),
            < 800  => ("Experto",      "🏆", 800),
            < 1200 => ("Maestro",      "💫", 1200),
            _      => ("Leyenda",      "🌟", int.MaxValue)
        };

        private static int CalculateStreak(List<DateTime> submittedDates)
        {
            if (submittedDates.Count == 0) return 0;
            // Cuenta actividades completadas en días distintos consecutivos (simplificado)
            var days = submittedDates.Select(d => d.Date).Distinct().OrderByDescending(d => d).ToList();
            int streak = 1;
            for (int i = 1; i < days.Count; i++)
            {
                if ((days[i - 1] - days[i]).TotalDays <= 7)
                    streak++;
                else
                    break;
            }
            return streak;
        }

        private static List<object> CalculateBadges(List<CompletedActivity> completed, int totalPoints, double averageScore, int totalAssigned)
        {
            var badges = new List<object>();

            // ── Insignias por cantidad de actividades ─────────────────────
            if (completed.Count >= 1)
                badges.Add(MakeBadge("primera_actividad", "Primera Actividad", "Completaste tu primera actividad", "🎯",
                    completed.Where(d => d.SubmittedAt.HasValue).Min(d => d.SubmittedAt)));

            if (completed.Count >= 3)
                badges.Add(MakeBadge("trio", "Trío Ganador", "Completaste 3 actividades", "🎪",
                    completed.OrderBy(d => d.SubmittedAt).Skip(2).First().SubmittedAt));

            if (completed.Count >= 5)
                badges.Add(MakeBadge("cinco_actividades", "Constante", "Completaste 5 actividades", "🔥",
                    completed.OrderBy(d => d.SubmittedAt).Skip(4).First().SubmittedAt));

            if (completed.Count >= 10)
                badges.Add(MakeBadge("diez_actividades", "Dedicado", "¡10 actividades completadas!", "💪",
                    completed.OrderBy(d => d.SubmittedAt).Skip(9).First().SubmittedAt));

            // ── Insignias por puntuación ──────────────────────────────────
            if (completed.Any(d => d.Score == 100))
                badges.Add(MakeBadge("puntaje_perfecto", "Puntaje Perfecto", "Obtuviste 100 en una actividad", "⭐",
                    completed.Where(d => d.Score == 100).Min(d => d.SubmittedAt)));

            if (completed.Count(d => d.Score == 100) >= 3)
                badges.Add(MakeBadge("perfeccionista", "Perfeccionista", "3 actividades con 100 puntos", "✨",
                    completed.Where(d => d.Score == 100).OrderBy(d => d.SubmittedAt).Skip(2).First().SubmittedAt));

            if (averageScore >= 80 && completed.Count >= 3)
                badges.Add(MakeBadge("alto_rendimiento", "Alto Rendimiento", "Promedio ≥ 80 con 3+ actividades", "🏆",
                    (DateTime?)DateTime.UtcNow));

            if (averageScore >= 90 && completed.Count >= 3)
                badges.Add(MakeBadge("excelencia", "Excelencia", "Promedio ≥ 90 con 3+ actividades", "🌟",
                    (DateTime?)DateTime.UtcNow));

            // ── Insignias por puntos totales ──────────────────────────────
            if (totalPoints >= 100)
                badges.Add(MakeBadge("cien_puntos", "Centenario", "Acumulaste 100 puntos", "💯",
                    (DateTime?)DateTime.UtcNow));

            if (totalPoints >= 500)
                badges.Add(MakeBadge("quinientos_puntos", "Acumulador", "Acumulaste 500 puntos", "💰",
                    (DateTime?)DateTime.UtcNow));

            // ── Insignia completista ───────────────────────────────────────
            if (totalAssigned > 0 && completed.Count == totalAssigned && totalAssigned >= 3)
                badges.Add(MakeBadge("completista", "Completista", "Completaste todas las actividades asignadas", "🎓",
                    (DateTime?)DateTime.UtcNow));

            return badges;
        }

        private static object MakeBadge(string id, string name, string description, string emoji, DateTime? earnedAt) =>
            new { id, name, description, emoji, earnedAt };

        // ── HU#15: Reporte general del curso ─────────────────────────────
        [HttpGet("{courseId}/report")]
        [Authorize(Roles = "Professor,Admin")]
        public async Task<IActionResult> GetCourseReport(int courseId)
        {
            var course = await _db.Courses
                .Include(c => c.CourseStudents)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return NotFound("Curso no encontrado");

            var activities = await _db.Activities
                .Include(a => a.Assignments)
                .Where(a => a.CourseId == courseId)
                .ToListAsync();

            var averageScores = new List<double>();
            var studentSummaries = new List<object>();

            foreach (var enrollment in course.CourseStudents)
            {
                var student = await _userManager.FindByIdAsync(enrollment.StudentId);
                if (student == null) continue;

                var assigned = activities
                    .Where(a => a.Assignments.Any(x => x.StudentId == enrollment.StudentId))
                    .ToList();

                var completed = assigned
                    .Where(a => a.Assignments.Any(x => x.StudentId == enrollment.StudentId && x.SubmittedAt.HasValue))
                    .ToList();

                var scores = completed
                    .Select(a => a.Assignments.First(x => x.StudentId == enrollment.StudentId).Score ?? 0)
                    .ToList();

                double studentAvg = scores.Count > 0 ? Math.Round(scores.Average(s => (double)s), 1) : 0;
                if (studentAvg > 0) averageScores.Add(studentAvg);

                // Calcular puntos ponderados
                double weightedPoints = 0;
                foreach (var act in completed)
                {
                    var asgn = act.Assignments.First(x => x.StudentId == enrollment.StudentId);
                    double basePoints = (asgn.Score ?? 0) * (act.PercentageValue / 100.0) * 10;
                    if (asgn.Score == 100) basePoints *= 1.20;
                    else if (asgn.Score >= 80) basePoints *= 1.10;
                    weightedPoints += basePoints;
                }
                var totalPoints = (int)Math.Round(weightedPoints);

                var (levelName, levelEmoji, _) = CalculateLevel(totalPoints);

                // Insignias del estudiante
                var completedActivities = completed.Select(a => new CompletedActivity
                {
                    Score = a.Assignments.First(x => x.StudentId == enrollment.StudentId).Score,
                    SubmittedAt = a.Assignments.First(x => x.StudentId == enrollment.StudentId).SubmittedAt,
                    ActivityTitle = a.Title
                }).ToList();

                var badges = CalculateBadges(completedActivities, totalPoints, studentAvg, assigned.Count);

                studentSummaries.Add(new
                {
                    studentId = enrollment.StudentId,
                    studentName = student.Name,
                    totalActivities = assigned.Count,
                    completedActivities = completed.Count,
                    averageScore = studentAvg,
                    totalPoints,
                    levelName,
                    levelEmoji,
                    badgeCount = badges.Count,
                    badges
                });
            }

            // Ordenar por puntos (ranking)
            var ranked = studentSummaries
                .OrderByDescending(s => ((dynamic)s).totalPoints)
                .Select((s, i) => new
                {
                    rank = i + 1,
                    studentId = ((dynamic)s).studentId,
                    studentName = ((dynamic)s).studentName,
                    totalActivities = ((dynamic)s).totalActivities,
                    completedActivities = ((dynamic)s).completedActivities,
                    averageScore = ((dynamic)s).averageScore,
                    totalPoints = ((dynamic)s).totalPoints,
                    levelName = ((dynamic)s).levelName,
                    levelEmoji = ((dynamic)s).levelEmoji,
                    badgeCount = ((dynamic)s).badgeCount,
                    badges = ((dynamic)s).badges
                })
                .ToList();

            return Ok(new
            {
                courseId,
                courseName = course.Name,
                totalStudents = course.CourseStudents.Count,
                totalActivities = activities.Count,
                averageCourseScore = averageScores.Count > 0 ? Math.Round(averageScores.Average(), 1) : 0,
                studentSummaries = ranked
            });
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