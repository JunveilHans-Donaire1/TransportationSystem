using System;
using System.ComponentModel.DataAnnotations;

namespace SportsTournament.Models
{
    public class StaffRequest
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [Display(Name = "Username")]
        [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [Display(Name = "Status")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Requested At")]
        [DataType(DataType.DateTime)]
        public DateTime RequestedAt { get; set; } = DateTime.Now;
    }
}
