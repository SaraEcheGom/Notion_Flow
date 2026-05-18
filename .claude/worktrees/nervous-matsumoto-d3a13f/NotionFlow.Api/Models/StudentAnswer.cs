using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotionFlow.Api.Models
{
    /// <summary>Stores each answer a student gave for a specific question in an activity.</summary>
    public class StudentAnswer
    {
        [Key]
        public int Id { get; set; }

        public int AssignmentId { get; set; }

        [ForeignKey(nameof(AssignmentId))]
        public ActivityAssignment? Assignment { get; set; }

        public int QuestionId { get; set; }

        [ForeignKey(nameof(QuestionId))]
        public ActivityQuestion? Question { get; set; }

        /// <summary>Comma-separated selected option IDs for MultipleChoice questions.</summary>
        [MaxLength(500)]
        public string? SelectedOptionIds { get; set; }

        /// <summary>Free text for OpenText questions.</summary>
        [MaxLength(4000)]
        public string? TextAnswer { get; set; }

        /// <summary>True = answered correctly (only meaningful for MultipleChoice).</summary>
        public bool IsCorrect { get; set; }
    }
}
