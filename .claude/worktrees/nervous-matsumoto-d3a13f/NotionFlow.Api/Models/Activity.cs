using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotionFlow.Api.Models
{
    public class Activity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

        /// <summary>Porcentaje sobre la nota del curso (0–100)</summary>
        public double PercentageValue { get; set; }

        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public Course? Course { get; set; }

        public ICollection<ActivityQuestion> Questions { get; set; } = new List<ActivityQuestion>();
        public ICollection<ActivityAssignment> Assignments { get; set; } = new List<ActivityAssignment>();
    }
}
