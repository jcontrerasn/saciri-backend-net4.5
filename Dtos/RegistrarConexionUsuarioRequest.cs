using System.ComponentModel.DataAnnotations;

namespace SACIRI_backend.Dtos
{
    public class RegistrarConexionUsuarioRequest
    {
        [Required]
        [StringLength(100)]
        public string Usuario { get; set; }

        [Required]
        [StringLength(100)]
        public string UsuarioAccion { get; set; }

        [StringLength(500)]
        public string Sustento { get; set; }
    }
}
