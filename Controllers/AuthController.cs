using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MottuProjeto.Data;
using MottuProjeto.DTOs;
using MottuProjeto.Models;

namespace MottuProjeto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Realiza login via cookie (sessão).</summary>
        /// <remarks>
        /// Envie <c>username</c> e <c>password</c>.  
        /// No momento, a senha é comparada como texto puro (use hash em produção).
        ///
        /// Exemplo:
        /// 
        /// {
        ///   "username": "admin",
        ///   "password": "admin123"
        /// }
        /// </remarks>
        /// <param name="dto">Credenciais do usuário.</param>
        /// <response code="200">Login efetuado com sucesso.</response>
        /// <response code="400">Payload inválido.</response>
        /// <response code="401">Credenciais inválidas.</response>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var user = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user is null || user.PasswordHash != dto.Password)
                return Unauthorized(new { message = "Usuário ou senha inválidos." });

            // Cria as claims do usuário autenticado
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true, // mantém a sessão até expirar
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                });

            return Ok(new
            {
                message = "Login efetuado.",
                user = new { user.Id, user.Nome, user.Username, user.Email, user.Role }
            });
        }

        /// <summary>Encerra a sessão do usuário (logout).</summary>
        /// <response code="204">Logout realizado.</response>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return NoContent();
        }

        /// <summary>Retorna as informações básicas do usuário autenticado.</summary>
        /// <response code="200">Informações do usuário.</response>
        /// <response code="401">Não autenticado.</response>
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            return Ok(new
            {
                user = User.Identity?.Name,
                email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
            });
        }

        /// <summary>Endpoint de acesso negado (usado pela autenticação).</summary>
        /// <response code="403">Acesso negado.</response>
        [HttpGet("denied")]
        public IActionResult Denied() => Forbid();
    }
}
