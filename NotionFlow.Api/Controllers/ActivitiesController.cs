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
        private readonly ILogger<ActivitiesController> _logger;

        public ActivitiesController(
            AppDbContext db,
            UserManager<User> userManager,
            ILogger<ActivitiesController> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetActivities(int courseId)
        {
            var activities = await _db.Activities
                .AsNoTracking()
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

        [HttpPost]
        public async Task<IActionResult> CreateActivity(int courseId, [FromBody] CreateActivityDto dto)
        {
            try
            {
                var course = await _db.Courses.FindAsync(courseId);
                if (course == null)
                    return NotFound(new { error = $"Curso {courseId} no encontrado" });

                var userId = UserId();
                var isTeacher = await _db.CourseTeachers
                    .AnyAsync(ct => ct.CourseId == courseId && ct.TeacherId == userId);

                if (!isTeacher)
                    return StatusCode(403, new { error = "No eres profesor de este curso", userId, courseId });

                var activity = new Activity
                {
                    CourseId = courseId,
                    Title = dto.Title,
                    Description = dto.Description,
                    DueDate = dto.DueDate.HasValue
                        ? DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc)
                        : (DateTime?)null,
                    PercentageValue = dto.PercentageValue,
                    CreatedAt = DateTime.UtcNow,
                    Questions = dto.Questions.Select(q => new ActivityQuestion
                    {
                        QuestionText = q.Text,
                        Type = q.QuestionType == "OpenText"
                            ? QuestionType.ShortAnswer
                            : QuestionType.MultipleChoice,
                        Options = q.Options.Select(o => new ActivityOption
                        {
                            OptionText = o.Text,
                            IsCorrect = o.IsCorrect
                        }).ToList()
                    }).ToList()
                };

                _db.Activities.Add(activity);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Actividad creada: {ActivityId} en curso {CourseId}", activity.Id, courseId);
                return Ok(MapToDto(activity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CreateActivity para curso {CourseId}", courseId);
                return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
            }
        }

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

                activity.Title = dto.Title;
                activity.Description = dto.Description;
                activity.DueDate = dto.DueDate.HasValue
                    ? DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc)
                    : (DateTime?)null;
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
                        IsCorrect = o.IsCorrect
                    }).ToList()
                }).ToList();

                await _db.SaveChangesAsync();
                return Ok(MapToDto(activity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en UpdateActivity {ActivityId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

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
                _logger.LogError(ex, "Error en DeleteActivity {ActivityId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

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
                        Status = ActivityStatus.Pending
                    });
                    assigned++;
                }

                await _db.SaveChangesAsync();
                return Ok(new { message = "Asignación completada", assigned, skipped });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en AssignActivity {ActivityId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

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

                var enrolled = await _db.CourseStudents
                    .AnyAsync(cs => cs.CourseId == courseId && cs.StudentId == userId);
                if (!enrolled)
                    return StatusCode(403, new { error = "No estás matriculado en este curso" });

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
                    await _db.SaveChangesAsync();
                }

                if (assignment.Status == ActivityStatus.Submitted || assignment.Status == ActivityStatus.Graded)
                    return BadRequest(new { error = "Ya enviaste esta actividad." });

                var existingAnswers = await _db.StudentAnswers
                    .Where(sa => sa.AssignmentId == assignment.Id)
                    .ToListAsync();
                _db.StudentAnswers.RemoveRange(existingAnswers);

                int correct = 0, total = 0;
                var savedAnswers = new List<QuestionResultDto>();

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

                    var correctIds = question.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToList();
                    savedAnswers.Add(new QuestionResultDto(
                        question.Id,
                        question.QuestionText,
                        question.Type == QuestionType.ShortAnswer ? "OpenText" : "MultipleChoice",
                        isCorrect,
                        answer.SelectedOptionIds ?? new List<int>(),
                        correctIds,
                        question.Options.Select(o => new OptionResponseDto(o.Id, o.OptionText, o.IsCorrect)).ToList(),
                        answer.TextAnswer
                    ));
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
                _logger.LogError(ex, "Error en SubmitActivity {ActivityId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

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
                foreach (var asgn in activity.Assignments.Where(a =>
                    a.Status == ActivityStatus.Submitted || a.Status == ActivityStatus.Graded))
                {
                    var studentAnswers = await _db.StudentAnswers
                        .AsNoTracking()
                        .Where(sa => sa.AssignmentId == asgn.Id)
                        .ToListAsync();

                    // Tipado con QuestionResultDto — sin Reflection
                    var questionResults = activity.Questions.Select(q =>
                    {
                        var sa = studentAnswers.FirstOrDefault(a => a.QuestionId == q.Id);
                        var selectedIds = sa?.SelectedOptionIds?
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => int.TryParse(s, out var n) ? n : 0)
                            .Where(n => n > 0)
                            .ToList() ?? new List<int>();

                        return new QuestionResultDto(
                            q.Id,
                            q.QuestionText,
                            q.Type == QuestionType.ShortAnswer ? "OpenText" : "MultipleChoice",
                            sa?.IsCorrect ?? false,
                            selectedIds,
                            q.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToList(),
                            q.Options.Select(o => new OptionResponseDto(o.Id, o.OptionText, o.IsCorrect)).ToList(),
                            sa?.TextAnswer
                        );
                    }).ToList();

                    var mcTotal = activity.Questions.Count(q => q.Type == QuestionType.MultipleChoice);
                    var mcCorrect = questionResults.Count(q => q.QuestionType == "MultipleChoice" && q.IsCorrect);

                    results.Add(new
                    {
                        studentId = asgn.StudentId,
                        studentName = asgn.Student?.Name ?? asgn.StudentId,
                        studentEmail = asgn.Student?.Email ?? "",
                        submittedAt = asgn.SubmittedAt,
                        score = asgn.Score,
                        correct = questionResults.Count(r => r.IsCorrect),
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
                _logger.LogError(ex, "Error en GetResults actividad {ActivityId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

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
            a.PercentageValue,
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
