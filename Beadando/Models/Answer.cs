using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Beadando.Models
{
    public class Answer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Text { get; set; }

        public bool IsCorrect { get; set; }

        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public virtual required Question Question { get; set; }

        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }
}