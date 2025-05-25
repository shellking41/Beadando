using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Beadando.Models
{
    public class Session
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public required string SessionKey { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual required User User { get; set; }
    }
}
