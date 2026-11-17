using System.ComponentModel.DataAnnotations;

namespace SACIRI_backend.Dtos
{
    public class CrearUsuarioRolRequest
    {
        [Required]
        [StringLength(100)]
        public string Usuario { get; set; }

        [Range(1, int.MaxValue)]
        public int IdRol { get; set; }

        [StringLength(100)]
        public string Jefe { get; set; }
    }
}
