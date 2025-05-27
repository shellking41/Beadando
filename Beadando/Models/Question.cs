using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beadando.Models
{
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Text { get; set; }

        public string? Image { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual List<Answer> Answers { get; set; } = new();
    }
}