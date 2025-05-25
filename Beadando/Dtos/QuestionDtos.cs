using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beadando.Dtos
{
    public class CreateQuestionRequest
    {
        [Required(ErrorMessage = "A kérdés szövege kötelező")]
        public required string Text { get; set; }
        
        public string? ImagePath { get; set; }
        
        [Required(ErrorMessage = "Legalább két válasz megadása kötelező")]
        [MinLength(2, ErrorMessage = "Legalább két válasz megadása kötelező")]
        public List<CreateAnswerRequest> Answers { get; set; } = new();
    }

    public class CreateAnswerRequest
    {
        [Required(ErrorMessage = "A válasz szövege kötelező")]
        public required string Text { get; set; }
        
        public bool IsCorrect { get; set; }
    }

    public class QuestionListResponse
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public string? ImagePath { get; set; }
        public int AnswerCount { get; set; }
    }

    public class QuestionDto
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public string? ImagePath { get; set; }
        public required List<AnswerDto> Answers { get; set; } = new();
    }

    public class AnswerDto
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class CreateQuestionDto
    {
        public required string Text { get; set; }
        public required List<CreateAnswerDto> Answers { get; set; } = new();
    }

    public class CreateAnswerDto
    {
        public required string Text { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class UpdateQuestionDto
    {
        public required string Text { get; set; }
    }
} 