using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MottuProjeto.Models
{
    /// <summary>
    /// Representa um usuário do sistema (autenticação e autorização).
    /// </summary>
    [Table("T_VM_USUARIO")]
    public class Usuario
    {
        /// <summary>
        /// Identificador único do usuário.
        /// </summary>
        [Key]
        [Column("ID_USUARIO")]
        public int Id { get; set; }

        /// <summary>
        /// Nome completo do usuário.
        /// </summary>
        [Required, StringLength(100)]
        [Column("NM_USUARIO")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// E-mail do usuário (deve ser único na base).
        /// </summary>
        [Required, StringLength(150), EmailAddress]
        [Column("DS_EMAIL")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nome de login (username) do usuário. Deve ser único.
        /// </summary>
        [Required, StringLength(60)]
        [Column("DS_USERNAME")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Senha armazenada como hash. Em produção utilize um algoritmo seguro (ex.: BCrypt).
        /// </summary>
        [Required]
        [Column("DS_PASSWORD_HASH")]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Perfil/Regra de acesso do usuário (ex.: Admin, User).
        /// </summary>
        [Required, StringLength(20)]
        [Column("DS_ROLE")]
        public string Role { get; set; } = "User";
    }
}
