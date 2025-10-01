using System.ComponentModel.DataAnnotations;

namespace MottuProjeto.DTOs
{
    public class MotoDTO
    {
        [Required]
        [StringLength(20)]
        public string Placa { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Modelo { get; set; } = string.Empty;

        [Required]
        public int IdArea { get; set; }
    }
}
