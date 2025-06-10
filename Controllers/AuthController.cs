using edusync_backend.Data;
using edusync_backend.DTOs;
using edusync_backend.Models;
using edusync_backend.Templates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using edusync_backend.Services;

namespace edusync_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly EduSyncDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthController(EduSyncDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login(LoginDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid email or password");
            }


            var token = GenerateJwtToken(user);

            return new AuthResponseDTO
            {
                Token = token,
                Role = user.Role,
                UserId = user.UserId,
                Name = user.Name
            };
        }
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDTO>> Register(RegisterDTO dto)
        {
            // Validate input fields
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length < 2 || dto.Name.Length > 50)
                return BadRequest("Name must be between 2 and 50 characters.");

            if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains("@"))
                return BadRequest("A valid email is required.");

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                return BadRequest("Password must be at least 6 characters long.");

            // Validate role
            var validRoles = new[] { "Student", "Instructor", "Admin" };
            if (!validRoles.Contains(dto.Role))
                return BadRequest("Invalid role. Must be Student, Instructor, or Admin.");

            // Check for existing email
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
                return Conflict("Email is already registered.");

            // Hash password
            var passwordHash = HashPassword(dto.Password);

            // Create new user
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = passwordHash,
                Role = dto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var token = GenerateJwtToken(user);
            try
            {
                var emailSubject = "Welcome to EduSync!";
                var emailBody = EmailTemplates.GetWelcomeEmailTemplate(user.Name);
                await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send welcome email: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }

            return new AuthResponseDTO
            {
                Token = token,
                Role = user.Role,
                UserId = user.UserId,
                Name = user.Name
            };
        }
        private string GenerateJwtToken(User user)
        {
            var tokenId = Guid.NewGuid().ToString();
            var issuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim("jti", tokenId),
                new Claim("iat", issuedAt),
                new Claim("sub", user.UserId.ToString()),
                new Claim("unique_name", user.Email),
                new Claim("UserId", user.UserId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                notBefore: DateTime.UtcNow,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            
            // Log token details
            // Console.WriteLine($"Generated new token for user {user.Email}:");
            // Console.WriteLine($"- Token ID (jti): {tokenId}");
            // Console.WriteLine($"- Issued At (iat): {issuedAt}");
            // Console.WriteLine($"- Expires At: {DateTime.UtcNow.AddHours(1)}");
            // Console.WriteLine($"- Role: {user.Role}");
            
            return tokenString;
        }

        // Constants for PBKDF2
        private const int SaltSize = 16; // 128 bit
        private const int HashSize = 32; // 256 bit
        private const int Iterations = 100000; // Number of iterations

        private string HashPassword(string password)
        {
            // Generate a salt
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            // Hash the password
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);

                // Combine salt and hash
                byte[] combinedBytes = new byte[SaltSize + HashSize];
                Buffer.BlockCopy(salt, 0, combinedBytes, 0, SaltSize);
                Buffer.BlockCopy(hash, 0, combinedBytes, SaltSize, HashSize);

                return Convert.ToBase64String(combinedBytes);
            }
        }

        private bool VerifyPassword(string password, string storedPasswordHash)
        {
            try
            {
                byte[] combinedBytes = Convert.FromBase64String(storedPasswordHash);

                // Extract salt and hash
                byte[] salt = new byte[SaltSize];
                byte[] storedHash = new byte[HashSize];
                Buffer.BlockCopy(combinedBytes, 0, salt, 0, SaltSize);
                Buffer.BlockCopy(combinedBytes, SaltSize, storedHash, 0, HashSize);

                // Hash the input password using the extracted salt
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] computedHash = pbkdf2.GetBytes(HashSize);
                    return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
                }
            }
            catch
            {
                // Handle exceptions (e.g., invalid Base64 string, incorrect format)
                return false;
            }
        }
    }
}
