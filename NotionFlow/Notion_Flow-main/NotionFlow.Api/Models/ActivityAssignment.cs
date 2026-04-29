using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotionFlow.Api.Models
{
    public class ActivityAssignment
    {
        [Key]
        public int Id { get; set; }

        public ActivityStatus Status { get; set; } = ActivityStatus.Pending;

        public DateTime? StartedAt { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public int? Score { get; set; }

        [MaxLength(2000)]
        public string? Feedback { get; set; }

        public int ActivityId { get; set; }

        [ForeignKey(nameof(ActivityId))]
        public Activity? Activity { get; set; }

        public string StudentId { get; set; } = string.Empty;

        [ForeignKey(nameof(StudentId))]
        public User? Student { get; set; }
    }

    public enum ActivityStatus
    {
        Pending,
        InProgress,
        Submitted,
        Graded,
        Late
    }
}