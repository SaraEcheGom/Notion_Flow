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
    [Route("api/courses/{courseId}/activities")]
    [Authorize]
    public class ActivitiesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<User> _userManager;

        public ActivitiesController(AppDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ── GET (abierto a cualquier usuario autenticado) ────────────────────

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetActivities(int courseId)
        {
            var activities = await _db.Activities
                .Where(a => a.CourseId == courseId)
                .Include(a => a.Questions).ThenInclude(q => q.Options)
                .Include(a => a.Assignments)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Ok(activities.Select(MapToDto));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActivity(int courseId, int id)
        {
            var activity = await FindAsync(courseId, id);
            if (activity == null) return NotFound();
            return Ok(MapToDto(activity));
        }

        // ── POST: Crear actividad (HU #5) ────────────────────────────────────
        // Sin restricción de rol para que funcione con cualquier valor que tenga el JWT
        [HttpPost]
        public async Task<IActionResult> CreateActivity(int courseId, [FromBody] CreateActivityDto dto)
        {
            try
            {
                Console.WriteLine($"\n[Activities] POST courseId={courseId}");

                var course = await _db.Courses.FindAsync(courseId);
                if (course == null)
                    return NotFound(new { error = $"Curso {courseId} no encontrado" });

                var userId = UserId();
                Console.WriteLine($"[Activities] userId={userId}");

                // Verificar que el usuario es profesor de este curso
                var isTeacher = await _db.CourseTeachers
                    .AnyAsync(ct => ct.CourseId == courseId && ct.TeacherId == userId);

                Console.WriteLine($"[Activities] isTeacher={isTeacher}");

                if (!isTeacher)
                    return StatusCode(403, new { error = "No eres profesor de este curso", userId, courseId });

                var activity = new Activity
                {
                    CourseId    = courseId,
                    Title       = dto.Title,
                    Description = dto.Description,
                    DueDate     = dto.DueDate.HasValue ? DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc) : (DateTime?)null,
                    PercentageValue = dto.PercentageValue,
                    CreatedAt   = DateTime.UtcNow,
                    Questions   = dto.Questions.Select(q => new ActivityQuestion
                    {
                        QuestionText = q.Text,
                        Type = q.QuestionType == "OpenText"
                            ? QuestionType.ShortAnswer
                            : QuestionType.MultipleChoice,
                        Options = q.Options.Select(o => new ActivityOption
                        {
                            OptionText = o.Text,
                            IsCorrect  = o.IsCorrect
                        }).ToList()
                    }).ToList()
                };

                _db.Activities.Add(activity);
                await _db.SaveChangesAsync();

                Console.WriteLine($"[Activities] Created id={activity.Id}");
                return Ok(MapToDto(activity));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Activities] ERROR: {ex.Message}\n{ex.InnerException?.Message}");
                return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        // ── PUT: Editar actividad (HU #6) ────────────────────────────────────
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateActivity(int courseId, int id, [FromBody] UpdateActivityDto dto)
        {
            try
            {
                var activity = await FindAsync(courseId, id);
                if (activity == null) return NotFound();

                var userId = UserId();
                var isTeacher = await _db.CourseTeachers
                    .AnyAsync(ct => ct.CourseId == courseId && ct.TeacherId == userId);
                if (!isTeacher)
                    return StatusCode(403, new { error = "No eres profesor de este curso" });

                activity.Title          = dto.Title;
                activity.Description    = dto.Description;
                activity.DueDate        = dto.DueDate.HasValue ? DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc) : (DateTime?)null;
                activity.PercentageValue = dto.PercentageValue;

                _db.ActivityQuestions.RemoveRange(activity.Questions);
                activity.Questions = dto.Questions.Select(q => new ActivityQuestion
                {
                    QuestionText = q.Text,
                    Type = q.QuestionType == "OpenText"
                        ? QuestionType.ShortAnswer
                        : QuestionType.MultipleChoice,
                    Options = q.Options.Select(o => new ActivityOption
                    {
                        OptionText = o.Text,
                        IsCorrect  = o.IsCorrect
                    }).ToList()
                }).ToList();

                await _db.SaveChangesAsync();
                return Ok(MapToDto(activity));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Activities] UpdateActivity ERROR: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ── DELETE: Eliminar actividad (HU #7) ───────────────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivity(int courseId, int id)
        {
            try
            {
                var activity = await FindAsync(courseId, id);
                if (activity == null) return NotFound();

                var userId = UserId();
                var isTeacher = await _db.CourseTeachers
                    .AnyAsync(ct => ct.CourseId == courseId && ct.TeacherId == userId);
                if (!isTeacher)
                    return StatusCode(403, new { error = "No eres profesor de este curso" });

                _db.Activities.Remove(activity);
                await _db.SaveChangesAsync();
                return Ok(new { message = "Actividad eliminada", id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Activities] DeleteActivity ERROR: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ── POST /{id}/assign: Asignar actividad (HU #8) ─────────────────────
        [HttpPost("{id}/assign")]
        public async Task<IActionResult> AssignActivity(int courseId, int id, [FromBody] AssignActivityDto dto)
        {
            try
            {
                var activity = await _db.Activities
                    .Include(a => a.Assignments)
                    .FirstOrDefaultAsync(a => a.Id == id && a.CourseId == courseId);

                if (activity == null) return NotFound();

                var userId = UserId();
                var isTeacher = await _db.CourseTeachers
                    .AnyAsync(ct => ct.CourseId == courseId && ct.TeacherId == userId);
                if (!isTeacher)
                    return StatusCode(403, new { error = "No eres profesor de este curso" });

                int assigned = 0, skipped = 0;
                foreach (var studentId in dto.StudentIds)
                {
                    var enrolled = await _db.CourseStudents
                        .AnyAsync(cs => cs.CourseId == courseId && cs.StudentId == studentId);
                    if (!enrolled) { skipped++; continue; }

                    if (activity.Assignments.Any(a => a.StudentId == studentId)) { skipped++; continue; }

                    activity.Assignments.Add(new ActivityAssignment
                    {
                        StudentId = studentId,
                        Status    = ActivityStatus.Pending
                    });
                    assigned++;
                }

                await _db.SaveChangesAsync();
                return Ok(new { message = "Asignación completada", assigned, skipped });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Activities] AssignActivity ERROR: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ── POST /{id}/submit: Estudiante responde la actividad ───────────────
        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitActivity(int courseId, int id, [FromBody] SubmitActivityDto dto)
        {
            try
            {
                var activity = await _db.Activities
                    .Include(a => a.Questions).ThenInclude(q => q.Options)
                    .Include(a => a.Assignments)
                    .FirstOrDefaultAsync(a => a.Id == id && a.CourseId == courseId);

                if (activity == null) return NotFound(new { error = "Actividad no encontrada" });

                var userId = UserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "Usuario no autenticado" });

                // Verify student is enrolled in the course
                var enrolled = await _db.CourseStudents
                    .AnyAsync(cs => cs.CourseId == courseId && cs.StudentId == userId);
                if (!enrolled)
                    return StatusCode(403, new { error = "No estás matriculado en este curso" });

                // Get or create assignment for this student
                var assignment = activity.Assignments.FirstOrDefault(a => a.StudentId == userId);
                if (assignment == null)
                {
                    assignment = new ActivityAssignment
                    {
                        ActivityId = id,
                        StudentId = userId,
                        Status = ActivityStatus.Pending,
                    };
                    _db.ActivityAssignments.Add(assignment);
                    await _db.SaveChangesAsync(); // need ID for StudentAnswers FK
                }

                if (assignment.Status == ActivityStatus.Submitted || assignment.Status == ActivityStatus.Graded)
                    return BadRequest(new { error = "Ya enviaste esta actividad." });

                // Remove any previous partial answers
                var existingAnswers = await _db.StudentAnswers
                    .Where(sa => sa.AssignmentId == assignment.Id)
                    .ToListAsync();
                _db.StudentAnswers.RemoveRange(existingAnswers);

                // Save per-question answers and auto-grade
                int correct = 0, total = 0;
                var savedAnswers = new List<object>();

                foreach (var answer in dto.Answers)
                {
                    var question = activity.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                    if (question == null) continue;

                    bool isCorrect = false;
                    if (question.Type == QuestionType.MultipleChoice)
                    {
                        total++;
                        var correctOptionIds = question.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToHashSet();
                        var selectedIds = (answer.SelectedOptionIds ?? new List<int>()).ToHashSet();
                        isCorrect = correctOptionIds.SetEquals(selectedIds);
                        if (isCorrect) correct++;
                    }

                    var studentAnswer = new StudentAnswer
                    {
                        AssignmentId = assignment.Id,
                        QuestionId = answer.QuestionId,
                        SelectedOptionIds = answer.SelectedOptionIds != null
                            ? string.Join(",", answer.SelectedOptionIds)
                            : null,
                        TextAnswer = answer.TextAnswer,
                        IsCorrect = isCorrect,
                    };
                    _db.StudentAnswers.Add(studentAnswer);

                    // Build result per question for the response
                    var correctIds = question.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToList();
                    savedAnswers.Add(new
                    {
                        questionId = question.Id,
                        questionText = question.QuestionText,
                        questionType = question.Type == QuestionType.ShortAnswer ? "OpenText" : "MultipleChoice",
                        isCorrect,
                        selectedOptionIds = answer.SelectedOptionIds ?? new List<int>(),
                        correctOptionIds = correctIds,
                        options = question.Options.Select(o => new
                        {
                            id = o.Id,
                            text = o.OptionText,
                            isCorrect = o.IsCorrect,
                        }).ToList(),
                        textAnswer = answer.TextAnswer,
                    });
                }

                assignment.SubmittedAt = DateTime.UtcNow;
                assignment.Status = ActivityStatus.Submitted;
                if (total > 0)
                    assignment.Score = (int)Math.Round((double)correct / total * 100);

                await _db.SaveChangesAsync();

                return Ok(new
                {
                    message = "Actividad enviada correctamente",
                    score = assignment.Score,
                    correct,
                    total,
                    answers = savedAnswers,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Activities] SubmitActivity ERROR: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ── GET /{id}/results: Profesor ve resultados de todos los estudiantes ─
        [HttpGet("{id}/results")]
        public async Task<IActionResult> GetResults(int courseId, int id)
        {
            try
            {
                var userId = UserId();
                var isTeacher = await _db.CourseTeachers
                    .AnyAsync(ct => ct.CourseId == courseId && ct.TeacherId == userId);
                if (!isTeacher)
                    return StatusCode(403, new { error = "Solo el profesor puede ver los resultados" });

                var activity = await _db.Activities
                    .Include(a => a.Questions).ThenInclude(q => q.Options)
                    .Include(a => a.Assignments).ThenInclude(asgn => asgn.Student)
                    .FirstOrDefaultAsync(a => a.Id == id && a.CourseId == courseId);

                if (activity == null) return NotFound();

                var results = new List<object>();
                foreach (var asgn in activity.Assignments.Where(a => a.Status == ActivityStatus.Submitted || a.Status == ActivityStatus.Graded))
                {
                    var studentAnswers = await _db.StudentAnswers
                        .Where(sa => sa.AssignmentId == asgn.Id)
                        .ToListAsync();

                    var questionResults = activity.Questions.Select(q =>
                    {
                        var sa = studentAnswers.FirstOrDefault(a => a.QuestionId == q.Id);
                        var selectedIds = sa?.SelectedOptionIds?
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => int.TryParse(s, out var n) ? n : 0)
                            .Where(n => n > 0)
                            .ToList() ?? new List<int>();

                        return new
                        {
                            questionId = q.Id,
                            questionText = q.QuestionText,
                            questionType = q.Type == QuestionType.ShortAnswer ? "OpenText" : "MultipleChoice",
                            isCorrect = sa?.IsCorrect ?? false,
                            selectedOptionIds = selectedIds,
                            correctOptionIds = q.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToList(),
                            options = q.Options.Select(o => new
                            {
                                id = o.Id,
                                text = o.OptionText,
                                isCorrect = o.IsCorrect,
                            }).ToList(),
                            textAnswer = sa?.TextAnswer,
                        };
                    }).ToList();

                    var mcTotal = activity.Questions.Count(q => q.Type == QuestionType.MultipleChoice);
                    var mcCorrect = studentAnswers.Count(sa =>
                    {
                        var q = activity.Questions.FirstOrDefault(q => q.Id == sa.QuestionId);
                        return q?.Type == QuestionType.MultipleChoice && sa.IsCorrect;
                    });

                    results.Add(new
                    {
                        studentId = asgn.StudentId,
                        studentName = asgn.Student?.Name ?? asgn.StudentId,
                        studentEmail = asgn.Student?.Email ?? "",
                        submittedAt = asgn.SubmittedAt,
                        score = asgn.Score,
                        correct = mcCorrect,
                        total = mcTotal,
                        questions = questionResults,
                    });
                }

                return Ok(new
                {
                    activityId = activity.Id,
                    activityTitle = activity.Title,
                    totalStudents = activity.Assignments.Count,
                    submitted = results.Count,
                    results,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Activities] GetResults ERROR: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private string? UserId() =>
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        private async Task<Activity?> FindAsync(int courseId, int id) =>
            await _db.Activities
                .Include(a => a.Questions).ThenInclude(q => q.Options)
                .Include(a => a.Assignments)
                .FirstOrDefaultAsync(a => a.Id == id && a.CourseId == courseId);

        private static ActivityResponseDto MapToDto(Activity a) => new(
            a.Id,
            a.CourseId,
            a.Title,
            a.Description,
            a.DueDate,
            a.PercentageValue,   // ← ahora usa el valor real
            a.CreatedAt,
            a.Questions.Select(q => new QuestionResponseDto(
                q.Id,
                q.QuestionText,
                q.Type == QuestionType.ShortAnswer ? "OpenText" : "MultipleChoice",
                q.Options.Select(o => new OptionResponseDto(o.Id, o.OptionText, o.IsCorrect)).ToList()
            )).ToList(),
            a.Assignments.Select(asgn => new AssignmentResponseDto(
                asgn.Id,
                asgn.StudentId,
                asgn.StartedAt ?? DateTime.UtcNow
            )).ToList()
        );
    }
}
