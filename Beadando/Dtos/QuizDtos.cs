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
        public List<QuizQuestionResultDto> Questions { get; set; } = new();
    }

    public class QuizQuestionResultDto
    {
        public int QuestionId { get; set; }
        public required string QuestionText { get; set; }
        public string? QuestionImage { get; set; }
        public required string UserAnswerText { get; set; }
        public bool IsCorrect { get; set; }
        public required string CorrectAnswerText { get; set; }
    }

    public class QuizDetailsDto
    {
        public int Id { get; set; }
        public DateTime CompletedAt { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public bool Passed { get; set; }
        public List<QuizQuestionResultDto> Questions { get; set; } = new();
    }
} 