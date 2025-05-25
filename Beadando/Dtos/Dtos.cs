using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Beadando.Dtos
{
   
    public class RegisterRequest
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    
    public class QuizCreateRequest
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
    }

    public class QuizResponse
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

  
    public class QuizStartResponse
    {
        public int UserQuizResultId { get; set; }
        public required string QuizTitle { get; set; }
        public List<QuestionResponse> Questions { get; set; } = new();
    }

    
    public class QuestionCreateRequest
    {
        public required string Text { get; set; }
        public List<AnswerCreateRequest> Answers { get; set; } = new();
    }

    public class QuestionResponse
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public List<AnswerResponse> Answers { get; set; } = new();
    }

    
    public class AnswerCreateRequest
    {
        public required string Text { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class AnswerResponse
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public bool IsCorrect { get; set; }
    }


    public class AnswerSubmissionRequest
    {
        public int UserQuizResultId { get; set; }
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }
    }

  
    public class QuizResultResponse
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public required string QuizTitle { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
    }
}