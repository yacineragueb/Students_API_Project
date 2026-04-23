using StudentAPIDataAccessLayer;

namespace StudentApi.DTOs.Auth
{
    public class RefreshTokenResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
