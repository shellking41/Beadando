using System;

namespace Beadando.Models
{
    public class UserAnswer
    {
        public int Id { get; set; }
        public int UserQuizResultId { get; set; }
        public required UserQuizResult UserQuizResult { get; set; }
        public int QuestionId { get; set; }
        public required Question Question { get; set; }
        public int AnswerId { get; set; }
        public required Answer Answer { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}