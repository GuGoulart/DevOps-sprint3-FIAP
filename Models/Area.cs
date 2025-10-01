using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MottuProjeto.Models
{
    /// <summary>
    /// Representa uma área/região operacional.
    /// </summary>
    [Table("T_VM_AREA")]
    public class Area
    {
        /// <summary>
        /// Identificador único da área.
        /// </summary>
        [Key]
        [Column("ID_AREA")]
        public int Id { get; set; }

        /// <summary>
        /// Nome da área (ex.: Centro, Zona Leste).
        /// </summary>
        [Required]
        [StringLength(100)]
        [Column("NM_AREA")]
        public string Nome { get; set; } = string.Empty;
    }
}
