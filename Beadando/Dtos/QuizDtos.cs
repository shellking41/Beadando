using System;
using System.Collections.Generic;

namespace Beadando.Dtos
{
    public class UserAnswerSubmissionDto
    {
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }
    }

    public class QuizResultDto
    {
        public required string QuizTitle { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public bool Passed { get; set; }
        public DateTime CompletedAt { get; set; }
    }
} 