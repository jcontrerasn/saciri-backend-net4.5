using System.ComponentModel.DataAnnotations;

namespace SACIRI_backend.Dtos
{
    public class CrearPuestoRequest
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(250)]
        public string Descripcion { get; set; }
    }
}
