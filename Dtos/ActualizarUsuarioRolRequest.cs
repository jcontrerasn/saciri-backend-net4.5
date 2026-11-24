using System.ComponentModel.DataAnnotations;

namespace SACIRI_backend.Dtos
{
    public class ActualizarUsuarioRolRequest
    {
        [Range(1, int.MaxValue)]
        public int IdRol { get; set; }

        [StringLength(100)]
        public string Jefe { get; set; }

        public string Puestos { get; set; }
    }
}
