using System.ComponentModel.DataAnnotations;

namespace MottuProjeto.DTOs
{
    public class UsuarioDTO
    {
        [Required, StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required, StringLength(150), EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
