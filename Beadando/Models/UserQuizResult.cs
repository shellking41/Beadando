using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Beadando.Models
{
    public class UserQuizResult
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public required User User { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public int Score { get; set; }
        public int TotalQuestions { get; set; }

        public virtual List<UserAnswer> UserAnswers { get; set; } = new();
    }
}