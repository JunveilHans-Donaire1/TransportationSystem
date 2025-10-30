namespace SportsTournament.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!; // Plain text for now (for demo)
        public string Role { get; set; }  =null!; // Default role is User
    }
}
