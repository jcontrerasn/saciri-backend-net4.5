using System.ComponentModel.DataAnnotations;

namespace SACIRI_backend.Dtos
{
    public class UsuarioRolFiltroRequest
    {
        [Required]
        [StringLength(100)]
        public string Usuario { get; set; }
    }
}
