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

        // ── Cursos ────────────────────────────────────────────────────────────

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

        // ── Estudiantes en curso ──────────────────────────────────────────────

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

        // ── Evaluaciones ──────────────────────────────────────────────────────

        [HttpGet("{courseId}/evaluations")]
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

            _logger.LogInformation("Evaluación creada en curso {CourseId} por {UserId}", courseId, userId);
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

            var isTeacherOfCourse = await _db.CourseTeachers
                .AnyAsync(ct => ct.CourseId == evaluation.CourseId && ct.TeacherId == userId);
            if (!isTeacherOfCourse)
                return StatusCode(403, new { error = "No eres profesor del curso de esta evaluación" });

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
            _logger.LogInformation("Nota guardada para {StudentId} en evaluación {EvaluationId}", dto.StudentId, evaluationId);
            return Ok(grade);
        }

        // ── Contenidos ────────────────────────────────────────────────────────

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

        // ── HU#13 & HU#14: Progreso individual de un estudiante ──────────────

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

            // ── Puntos ponderados (HU#16) ────────────────────────────────────
            double weightedPoints = 0;
            foreach (var act in completed)
            {
                double basePoints = (act.score ?? 0) * (act.percentageValue / 100.0) * 10;
                if (act.score == 100) basePoints *= 1.20;
                else if (act.score >= 80) basePoints *= 1.10;
                weightedPoints += basePoints;
            }
            var totalPoints = (int)Math.Round(weightedPoints);

            var averageScore = completed.Count > 0
                ? completed.Average(d => (double)(d.score ?? 0))
                : 0;

            var (levelName, levelEmoji, nextLevelPoints) = CalculateLevel(totalPoints);

            var badges = CalculateBadges(
                completed.Select(d => new CompletedActivity
                {
                    Score = d.score,
                    SubmittedAt = d.submittedAt,
                    ActivityTitle = d.activityTitle
                }).ToList(),
                totalPoints, averageScore, assignedActivities.Count);

            int streak = CalculateStreak(completed
                .Where(d => d.submittedAt.HasValue)
                .OrderByDescending(d => d.submittedAt)
                .Select(d => d.submittedAt!.Value)
                .ToList());

            _logger.LogInformation("Progreso consultado — curso {CourseId} estudiante {StudentId}", courseId, studentId);

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

        // ── HU#15: Reporte general del curso ─────────────────────────────────

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

                var completedActs = assigned
                    .Where(a => a.Assignments.Any(x => x.StudentId == enrollment.StudentId && x.SubmittedAt.HasValue))
                    .ToList();

                var scores = completedActs
                    .Select(a => a.Assignments.First(x => x.StudentId == enrollment.StudentId).Score ?? 0)
                    .ToList();

                double studentAvg = scores.Count > 0 ? Math.Round(scores.Average(s => (double)s), 1) : 0;
                if (studentAvg > 0) averageScores.Add(studentAvg);

                double weightedPoints = 0;
                foreach (var act in completedActs)
                {
                    var asgn = act.Assignments.First(x => x.StudentId == enrollment.StudentId);
                    double basePoints = (asgn.Score ?? 0) * (act.PercentageValue / 100.0) * 10;
                    if (asgn.Score == 100) basePoints *= 1.20;
                    else if (asgn.Score >= 80) basePoints *= 1.10;
                    weightedPoints += basePoints;
                }
                var totalPoints = (int)Math.Round(weightedPoints);

                var (levelName, levelEmoji, _) = CalculateLevel(totalPoints);

                var completedActivities = completedActs.Select(a => new CompletedActivity
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
                    completedActivities = completedActs.Count,
                    averageScore = studentAvg,
                    totalPoints,
                    levelName,
                    levelEmoji,
                    badgeCount = badges.Count,
                    badges
                });
            }

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

            _logger.LogInformation("Reporte consultado — curso {CourseId}", courseId);

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

        // ── Helpers privados ──────────────────────────────────────────────────

        private record CompletedActivity
        {
            public int? Score { get; init; }
            public DateTime? SubmittedAt { get; init; }
            public string ActivityTitle { get; init; } = "";
        }

        private static (string Name, string Emoji, int NextLevel) CalculateLevel(int points) => points switch
        {
            < 50 => ("Principiante", "🌱", 50),
            < 150 => ("Aprendiz", "📚", 150),
            < 300 => ("Intermedio", "⚡", 300),
            < 500 => ("Avanzado", "🔥", 500),
            < 800 => ("Experto", "🏆", 800),
            < 1200 => ("Maestro", "💫", 1200),
            _ => ("Leyenda", "🌟", int.MaxValue)
        };

        private static int CalculateStreak(List<DateTime> submittedDates)
        {
            if (submittedDates.Count == 0) return 0;
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

        private static List<object> CalculateBadges(
            List<CompletedActivity> completed, int totalPoints, double averageScore, int totalAssigned)
        {
            var badges = new List<object>();

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

            if (totalPoints >= 100)
                badges.Add(MakeBadge("cien_puntos", "Centenario", "Acumulaste 100 puntos", "💯",
                    (DateTime?)DateTime.UtcNow));

            if (totalPoints >= 500)
                badges.Add(MakeBadge("quinientos_puntos", "Acumulador", "Acumulaste 500 puntos", "💰",
                    (DateTime?)DateTime.UtcNow));

            if (totalAssigned > 0 && completed.Count == totalAssigned && totalAssigned >= 3)
                badges.Add(MakeBadge("completista", "Completista", "Completaste todas las actividades asignadas", "🎓",
                    (DateTime?)DateTime.UtcNow));

            return badges;
        }

        private static object MakeBadge(string id, string name, string description, string emoji, DateTime? earnedAt) =>
            new { id, name, description, emoji, earnedAt };
    }
}