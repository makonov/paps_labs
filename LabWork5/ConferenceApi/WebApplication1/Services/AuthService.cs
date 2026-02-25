using ConferenceApi.Data;
using ConferenceApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ConferenceApi.Services
{
    public class AuthService
    {
        private readonly ConferenceDbContext _db;
        private readonly PasswordHasher<User> _hasher = new PasswordHasher<User>();
        private readonly string _secret;

        public AuthService(ConferenceDbContext db, IConfiguration config)
        {
            _db = db;
            _secret = config["Jwt:Secret"]!;
        }

        public string? Login(string username, string password, out string? role)
        {
            role = null;
            var user = _db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return null;

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed) return null;

            role = user.Role; // <-- сохраняем роль

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public void SeedAdmin()
        {
            if (_db.Users.Any()) return;

            var admin = new User
            {
                Username = "user1",
                Role = "organizer",
                PasswordHash = _hasher.HashPassword(null!, "pass123")
            };
            _db.Users.Add(admin);
            _db.SaveChanges();

            var speaker = new User
            {
                Username = "user2",
                Role = "speaker",
                PasswordHash = _hasher.HashPassword(null!, "pass123")
            };
            _db.Users.Add(speaker);
            _db.SaveChanges();

            var participant = new User
            {
                Username = "user3",
                Role = "participant",
                PasswordHash = _hasher.HashPassword(null!, "pass123")
            };
            _db.Users.Add(participant);
            _db.SaveChanges();
        }
    }
}