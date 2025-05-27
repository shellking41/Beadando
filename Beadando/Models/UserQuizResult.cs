using System;
using System.Collections.Generic;

namespace Beadando.Models
{
    public class UserQuizResult
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public required User User { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public List<UserAnswer> UserAnswers { get; set; } = new();
    }
}