using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beadando.Dtos
{
    public class QuestionListResponse
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public string? Image { get; set; }
        public int AnswerCount { get; set; }
    }

    public class QuestionDto
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public string? Image { get; set; }
        public required List<AnswerDto> Answers { get; set; } = new();
    }

    public class AnswerDto
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public bool IsCorrect { get; set; }
    }
} 