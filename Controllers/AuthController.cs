using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using StudentApi.DTOs.Auth;
using StudentAPIBusinessLayer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace StudentApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        private static string GenerateRefreshToken()
        {
            byte[] bytes = new byte[64];

            using RandomNumberGenerator randomNumber = RandomNumberGenerator.Create();
            randomNumber.GetBytes(bytes);

            return Convert.ToBase64String(bytes);
        }

        [HttpPost("login", Name = "Login")]
        [EnableRateLimiting("AuthLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public ActionResult Login([FromBody] DTOs.Auth.LoginRequest request)
        {
            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            Student? student = Student.Find(request.Email);

            if (student == null)
            {
                _logger.LogWarning("Failed login attempt (email not found). Email={Email}, IP={IP}", request.Email, ip);

                return Unauthorized("Invalid credentials");
            }

            bool IsValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, student.PasswordHash);

            if (!IsValidPassword)
            {
                _logger.LogWarning("Failed login attempt (bad password). Email={Email}, IP={IP}", request.Email, ip);

                return Unauthorized("Invalid credentials");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, student.ID.ToString()),
                new Claim(ClaimTypes.Email, student.Email),
                new Claim(ClaimTypes.Role, student.Role),
            };

            string? JWT_SECRET_KEY = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWT_SECRET_KEY));

            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
                (
                    issuer: "StudentApi",
                    audience: "StudentApiUsers",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: creds
                );

            string accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            string refreshToken = GenerateRefreshToken();

            student.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
            student.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            student.RefreshTokenRevokedAt = null;

            student.SaveRefreshToken();

            return Ok(new LoginResponse
            {
                Token = accessToken,
                User = student.StudentDTO,
                RefreshToken = refreshToken,
            });
        }


        [HttpPost("refresh")]
        [EnableRateLimiting("AuthLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public ActionResult Refresh([FromBody] RefreshTokenRequest request)
        {
            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            Student? student = Student.Find(request.Email);

            if (student == null)
            {
                _logger.LogWarning("Failed refresh token attempt (email not found). Email={Email}, IP={IP}", request.Email, ip);

                return Unauthorized("Invalid credentials");
            }

            if (student.RefreshTokenRevokedAt != null)
            {
                _logger.LogWarning("Failed refresh token attempt (refresh token revoked). Email={Email}, IP={IP}", request.Email, ip);

                return Unauthorized("Refresh token is revoked");
            }

            if (student.RefreshTokenExpiresAt == null || student.RefreshTokenExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning("Failed refresh token attempt (refresh token expired). Email={Email}, IP={IP}", request.Email, ip);

                return Unauthorized("Refresh token is expired");
            }

            bool isRefreshTokenValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, student.RefreshTokenHash);
            if(!isRefreshTokenValid)
            {
                _logger.LogWarning("Failed refresh token attempt (invalid refresh token). Email={Email}, IP={IP}", request.Email, ip);

                return Unauthorized("Invalid refresh token");
            }

            Claim[] claims =
            {
                new Claim(ClaimTypes.NameIdentifier, student.ID.ToString()),
                new Claim(ClaimTypes.Email, student.Email),
                new Claim(ClaimTypes.Role, student.Role),
            };

            string? JWT_SECRET_KEY = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWT_SECRET_KEY));

            SigningCredentials creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
                (
                    issuer: "StudentApi",
                    audience: "StudentApiUsers",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: creds
                );

            string accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            string refreshToken = GenerateRefreshToken();

            student.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
            student.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            student.RefreshTokenRevokedAt = null;

            student.SaveRefreshToken();

            return Ok(new RefreshTokenResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
            });
        }


        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult Logout([FromBody] LogoutRequest request )
        {
            Student? student = Student.Find(request.Email);

            if(student == null)
            {
                return Ok(); // Don't reveal if user exists
            }

            bool isRefreshTokenValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, student.RefreshTokenHash);
            if(!isRefreshTokenValid)
            {
                return Ok();
            }

            student.RefreshTokenRevokedAt = DateTime.UtcNow;

            student.SaveRefreshToken();
            return Ok("Logged out seccessfully");
        }
    }
}
