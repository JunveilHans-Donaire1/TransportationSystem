using System;
using System.ComponentModel.DataAnnotations;

namespace SportsTournament.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Message { get; set; } = null!;

        [Required]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        // Optional: To track which user this notification belongs to
        public string? Recipient { get; set; }
    }
}
