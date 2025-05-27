using System;
using System.Threading.Tasks;
using Beadando.Dtos;
using Microsoft.AspNetCore.Mvc;
using Beadando.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Beadando.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly SessionService _sessionService;
        private readonly ILogger<SessionController> _logger;

        public SessionController(SessionService sessionService, ILogger<SessionController> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Registration request received: {Email}", request.Email);
                
                if (string.IsNullOrEmpty(request.Name))
                {
                    _logger.LogWarning("Registration failed: Name is required");
                    return BadRequest("A név megadása kötelező");
                }

                if (string.IsNullOrEmpty(request.Email))
                {
                    _logger.LogWarning("Registration failed: Email is required");
                    return BadRequest("Az email cím megadása kötelező");
                }

                if (string.IsNullOrEmpty(request.Password))
                {
                    _logger.LogWarning("Registration failed: Password is required");
                    return BadRequest("A jelszó megadása kötelező");
                }

                var user = await _sessionService.RegisterUserAsync(request);
                _logger.LogInformation("User registered successfully: {Email}", request.Email);
                return Ok(new { message = "Sikeres regisztráció" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Registration failed: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration");
                return BadRequest("Váratlan hiba történt a regisztráció során");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var session = await _sessionService.LoginUserAsync(request);

               
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, session.UserId.ToString()),
                    new Claim(ClaimTypes.Name, session.User.Name),
                    new Claim(ClaimTypes.Email, session.User.Email)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

             
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = session.ExpiresAt
                    });

             
                Response.Cookies.Append("SessionKey", session.SessionKey, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, 
                    SameSite = SameSiteMode.Lax,
                    Expires = session.ExpiresAt
                });

                return Ok(new { 
                    message = "Sikeres bejelentkezés",
                    email = session.User.Email,
                    name = session.User.Name
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("validate")]
        public async Task<IActionResult> ValidateSession()
        {
            try
            {
                var session = await _sessionService.ValidateSessionAsync(Request, Response);
                return Ok(new { 
                    message = "Érvényes munkamenet", 
                    email = session.User.Email,
                    name = session.User.Name
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _sessionService.LogoutUserAsync(Request, Response);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Sikeres kijelentkezés" });
        }
    }
}
