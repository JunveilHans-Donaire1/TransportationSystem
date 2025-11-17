using System;
using System.ComponentModel.DataAnnotations;

namespace SportsTournament.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }

        // Link to a user (optional but recommended)
        public int? UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        // Rating from 1–5 (optional)
        [Range(1, 5)]
        public int? Rating { get; set; }

        // Optional metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Status of the feedback
    
        // Navigation property
        public User? User { get; set; }
    }
}
