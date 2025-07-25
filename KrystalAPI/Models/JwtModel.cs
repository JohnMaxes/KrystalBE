namespace KrystalAPI.Models
{
    public class JwtSettings
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }

    public class TokenObj
    {
        public string sub { get; set; }
        public string id { get; set; }
        public UserRole role { get; set; }
        public string jti { get; set; }
        public DateTime exp { get; set; }
        public string iss { get; set; }
        public string aud { get; set; }
    }

    public class RefreshTokenObj
    {

    }
}
