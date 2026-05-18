using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotionFlow.Api.Data;
using NotionFlow.Api.DTOs;
using NotionFlow.Api.Models;

namespace NotionFlow.Api.Controllers
{
    /// <summary>
    /// HU #5 Crear actividad tipo cuestionario
    /// HU #6 Editar actividad
    /// HU #7 Eliminar actividad
    /// HU #8 Asignar actividad
    /// </summary>
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

        // GET api/courses/{courseId}/activities
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetActivities(int courseId)
        {
            var activities = await _db.Activities
                .Where(a => a.CourseId == courseId)
                .Include(a => a.Questions)
                    .ThenInclude(q => q.Options)
                .Include(a => a.Assignments)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Ok(activities.Select(MapToDto));
        }

        // GET api/courses/{courseId}/activities/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]  
        public async Task<IActionResult> GetActivity(int courseId, int id)
        {
            var activity = await FindAsync(courseId, id);
            if (activity == null) return NotFound();
            return Ok(MapToDto(activity));
        }

        // POST api/courses/{courseId}/activities  — HU #5
        [HttpPost]
        [Authorize(Roles = "Professor")]
        public async Task<IActionResult> CreateActivity(int courseId, [FromBody] CreateActivityDto dto)
        {
            var course = await _db.Courses.FindAsync(courseId);
            if (course == null) return NotFound($"Curso {courseId} no encontrado");

            var userId = UserId();
            var isTeacher = await _db.CourseTeachers
                .AnyAsync(ct => ct.CourseId == courseId && ct.TeacherId == userId);
            if (!isTeacher) return Forbid();

            var activity = new Activity
            {
                CourseId = courseId,
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
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
            return Ok(MapToDto(activity));
        }

        // PUT api/courses/{courseId}/activities/{id}  — HU #6
        [HttpPut("{id}")]
        [Authorize(Roles = "Professor")]
        public async Task<IActionResult> UpdateActivity(int courseId, int id, [FromBody] UpdateActivityDto dto)
        {
            var activity = await FindAsync(courseId, id);
            if (activity == null) return NotFound();

            var userId = UserId();
            var isTeacher = await _db.CourseTeachers
                .AnyAsync(ct => ct.CourseId == courseId && ct.TeacherId == userId);
            if (!isTeacher) return Forbid();

            activity.Title = dto.Title;
            activity.Description = dto.Description;
            activity.DueDate = dto.DueDate;

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

        // DELETE api/courses/{courseId}/activities/{id}  — HU #7
        [HttpDelete("{id}")]
        [Authorize(Roles = "Professor")]
        public async Task<IActionResult> DeleteActivity(int courseId, int id)
        {
            var activity = await FindAsync(courseId, id);
            if (activity == null) return NotFound();

            var userId = UserId();
            var isTeacher = await _db.CourseTeachers
                .AnyAsync(ct => ct.CourseId == courseId && ct.TeacherId == userId);
            if (!isTeacher) return Forbid();

            _db.Activities.Remove(activity);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Actividad eliminada", id });
        }

        // POST api/courses/{courseId}/activities/{id}/assign  — HU #8
        [HttpPost("{id}/assign")]
        [Authorize(Roles = "Professor")]
        public async Task<IActionResult> AssignActivity(int courseId, int id, [FromBody] AssignActivityDto dto)
        {
            var activity = await _db.Activities
                .Include(a => a.Assignments)
                .FirstOrDefaultAsync(a => a.Id == id && a.CourseId == courseId);

            if (activity == null) return NotFound();

            var userId = UserId();
            var isTeacher = await _db.CourseTeachers
                .AnyAsync(ct => ct.CourseId == courseId && ct.TeacherId == userId);
            if (!isTeacher) return Forbid();

            int assigned = 0, skipped = 0;
            foreach (var studentId in dto.StudentIds)
            {
                var enrolled = await _db.CourseStudents
                    .AnyAsync(cs => cs.CourseId == courseId && cs.StudentId == studentId);
                if (!enrolled) { skipped++; continue; }

                var duplicate = activity.Assignments.Any(a => a.StudentId == studentId);
                if (duplicate) { skipped++; continue; }

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

        // ── Helpers ─────────────────────────────────────────────────────────

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
            0,   // PercentageValue not in model — frontend shows 0, acceptable
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
