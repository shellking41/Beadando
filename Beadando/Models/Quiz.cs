using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beadando.Models
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Title { get; set; }

        [Required]
        public required string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Question> Questions { get; set; } = new();

        public ICollection<UserQuizResult> Results { get; set; } = new List<UserQuizResult>();
    }
}