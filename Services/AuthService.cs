using K8Intel.Data;
using K8Intel.Dtos;
using K8Intel.Interfaces;
using K8Intel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace K8Intel.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<(User? user, string? token)> LoginAsync(UserLoginDto loginDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);
        if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            // Instead of throwing, we return nulls so the controller can handle it
            return (null, null);
        }

        var token = GenerateJwtToken(user);
        return (user, token);
    }

        public async Task<User> RegisterAsync(UserRegistrationDto registrationDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == registrationDto.Username))
            {
                throw new ArgumentException("Username already exists.");
            }

            var user = new User
            {
                Username = registrationDto.Username,
                PasswordHash = HashPassword(registrationDto.Password),
                Role = registrationDto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
            var key = Encoding.ASCII.GetBytes(jwtKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username ?? throw new InvalidOperationException("User.Username cannot be null")),
                    new Claim(ClaimTypes.Role, user.Role ?? string.Empty)
                }),
                Expires = DateTime.UtcNow.AddHours(5),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}