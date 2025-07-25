using KrystalAPI.Models;

namespace KrystalAPI.Data
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;

        public DateTime Expiration { get; set; }

        public bool IsRevoked { get; set; } = false;
        public int UserId { get; set; }
        public User User { get; set; }
    }
}