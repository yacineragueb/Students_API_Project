using StudentAPIDataAccessLayer;

namespace StudentApi.DTOs.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public StudentDTO User { get; set; }
        public string RefreshToken { get; set; }
    }
}
