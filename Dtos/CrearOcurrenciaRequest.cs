using System.ComponentModel.DataAnnotations;

namespace SACIRI_backend.Dtos
{
    public class CrearOcurrenciaRequest
    {
        [Range(1, int.MaxValue)]
        public int IdSubcategoria { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(250)]
        public string Descripcion { get; set; }
    }
}
