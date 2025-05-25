namespace Beadando.Services
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Http;
    using Beadando.Models;
    using Beadando.Dtos;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using Beadando.Data;

    public class SessionService
    {
        private readonly ApplicationDbContext _context;
        private const int SESSION_EXPIRY_DAYS = 14;
        private const int MIN_PASSWORD_LENGTH = 6;

        public SessionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> RegisterUserAsync(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("A név megadása kötelező.");
            }

            if (request.Name.Length < 2)
            {
                throw new ArgumentException("A név legalább 2 karakter hosszú kell legyen.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ArgumentException("Az email cím megadása kötelező.");
            }

            var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(request.Email, emailPattern))
            {
                throw new ArgumentException("Érvénytelen email cím formátum.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("A jelszó megadása kötelező.");
            }

            if (request.Password.Length < MIN_PASSWORD_LENGTH)
            {
                throw new ArgumentException($"A jelszó legalább {MIN_PASSWORD_LENGTH} karakter hosszú kell legyen.");
            }

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new ArgumentException("Ez az email cím már regisztrálva van.");
            }

            try
            {
                var user = new User
                {
                    Name = request.Name.Trim(),
                    Email = request.Email.Trim().ToLower(),
                    PasswordHash = HashPassword(request.Password),
                    CreatedAt = DateTime.UtcNow,
                    Sessions = new List<Session>(),
                    QuizResults = new List<UserQuizResult>()
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Hiba történt a felhasználó mentése közben.", ex);
            }
        }

        public async Task<Session> LoginUserAsync(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Hibás email vagy jelszó.");
            }

            var session = new Session
            {
                UserId = user.Id,
                User = user,
                SessionKey = GenerateSessionKey(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(SESSION_EXPIRY_DAYS),
                IsActive = true
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return session;
        }

        public async Task<Session> ValidateSessionAsync(HttpRequest request, HttpResponse response)
        {
            var sessionKey = request.Cookies["SessionKey"];
            if (string.IsNullOrEmpty(sessionKey))
            {
                throw new UnauthorizedAccessException("Nincs érvényes munkamenet.");
            }

            var session = await _context.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SessionKey == sessionKey && s.IsActive);

            if (session == null || session.ExpiresAt <= DateTime.UtcNow)
            {
                response.Cookies.Delete("SessionKey");
                throw new UnauthorizedAccessException("A munkamenet lejárt.");
            }

            return session;
        }

        public async Task LogoutUserAsync(HttpRequest request, HttpResponse response)
        {
            if (request.Cookies.TryGetValue("SessionKey", out string sessionKey))
            {
                var session = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionKey == sessionKey && s.IsActive);
                if (session != null)
                {
                    session.IsActive = false;
                    await _context.SaveChangesAsync();
                }
            }
            
            response.Cookies.Delete("SessionKey");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }

        private string GenerateSessionKey()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
