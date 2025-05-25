using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beadando.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string PasswordHash { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
        public virtual ICollection<UserQuizResult> QuizResults { get; set; } = new List<UserQuizResult>();
    }
}
