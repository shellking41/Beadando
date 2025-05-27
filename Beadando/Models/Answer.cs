using System;
using System.Collections.Generic;

namespace Beadando.Models
{
    public class Answer
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public bool IsCorrect { get; set; }
        public int QuestionId { get; set; }
        public required Question Question { get; set; }
        public List<UserAnswer> UserAnswers { get; set; } = new();
    }
}