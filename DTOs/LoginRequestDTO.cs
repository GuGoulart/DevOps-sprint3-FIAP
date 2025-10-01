using System.ComponentModel.DataAnnotations;

namespace MottuProjeto.DTOs
{
    public class LoginRequestDTO
    {
        [Required] public string Username { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }
}
