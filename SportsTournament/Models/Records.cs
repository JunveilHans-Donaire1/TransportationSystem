using System.ComponentModel.DataAnnotations;

namespace SportsTournament.Models
{
 

    public class Records
    {
        [Key]
        public string Id { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        public string Type { get; set; } = null!;

        [Required]
        public int Passenger { get; set; }

        [Required]
        public string Location { get; set; } = null!;

        [Required]
        public string Destination { get; set; } = null!;

        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected

        [Required]
        public string Username { get; set; } = null!;
        
        [Required]
        public int PhoneNumber { get; set; }
  
    }


}
