using System.ComponentModel.DataAnnotations;

namespace SACIRI_backend.Dtos
{
    public class UsuarioFiltroRequest
    {
        [Required]
        [StringLength(50)]
        public string Pfiltro_c { get; set; }
    }
}
