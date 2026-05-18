using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotionFlow.Api.Models
{
    public class ActivityQuestion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string QuestionText { get; set; } = string.Empty;

        public QuestionType Type { get; set; }

        public int Points { get; set; } = 1;

        public int Order { get; set; }

        public int ActivityId { get; set; }

        [ForeignKey(nameof(ActivityId))]
        public Activity? Activity { get; set; }

        public ICollection<ActivityOption> Options { get; set; } = new List<ActivityOption>();
    }

    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse,
        ShortAnswer,
        FillInTheBlank,
    }
}
