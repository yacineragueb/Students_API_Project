namespace StudentApi.DTOs.Auth
{
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
        public string Email { get; set; }
    }
}
