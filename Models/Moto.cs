using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MottuProjeto.Models
{
    [Table("T_VM_MOTO")]
    public class Moto
    {
        [Key]
        [Column("ID_MOTO")]
        public int Id { get; set; }

        [Required]
        [Column("DS_PLACA")]
        [StringLength(20)]
        public string Placa { get; set; } = string.Empty;

        [Required]
        [Column("NM_MODELO")]
        [StringLength(100)]
        public string Modelo { get; set; } = string.Empty;

        [Required]
        [Column("ID_AREA")]
        public int IdArea { get; set; }
    }
}
