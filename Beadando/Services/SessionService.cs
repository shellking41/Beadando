using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Beadando.Models;
using Beadando.Data;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;

namespace Beadando.Services
{
    public class SessionService
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordService _passwordService;
        private const int SESSION_LENGTH_DAYS = 30;

        public SessionService(ApplicationDbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        public async Task<Session> CreateSessionAsync(User user)
        {
            var token = GenerateToken();
            var session = new Session
            {
                Token = token,
                UserId = user.Id,
                User = user,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(SESSION_LENGTH_DAYS)
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return session;
        }

        public async Task<Session> ValidateSessionAsync(HttpRequest request, HttpResponse response)
        {
            var token = request.Cookies["session_token"];
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("No session token provided");
            }

            var session = await _context.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Token == token);

            if (session == null)
            {
                throw new UnauthorizedAccessException("Invalid session token");
            }

            if (session.ExpiresAt < DateTime.UtcNow)
            {
                _context.Sessions.Remove(session);
                await _context.SaveChangesAsync();
                throw new UnauthorizedAccessException("Session expired");
            }

            session.ExpiresAt = DateTime.UtcNow.AddDays(SESSION_LENGTH_DAYS);
            await _context.SaveChangesAsync();

            response.Cookies.Append("session_token", session.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(SESSION_LENGTH_DAYS)
            });

            return session;
        }

        public async Task<User> RegisterUserAsync(string name, string email, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                throw new InvalidOperationException("Email already registered");
            }

            var passwordHash = _passwordService.HashPassword(password);

            var user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> LoginUserAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            if (!_passwordService.VerifyPassword(password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            return user;
        }

        public async Task LogoutAsync(HttpRequest request)
        {
            var token = request.Cookies["session_token"];
            if (!string.IsNullOrEmpty(token))
            {
                var session = await _context.Sessions.FirstOrDefaultAsync(s => s.Token == token);
                if (session != null)
                {
                    _context.Sessions.Remove(session);
                    await _context.SaveChangesAsync();
                }
            }
        }

        private string GenerateToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }
    }
}
