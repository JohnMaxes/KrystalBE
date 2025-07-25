using KrystalAPI.Data;

namespace KrystalAPI.Models
{
    public enum UserRole { User, Moderator, Admin }
    public class User
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public UserInformation? Info { get; set; }
    };

    public class UserInformation
    {
        public int ID { get; set; }
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}


