using System.ComponentModel.DataAnnotations;

namespace SACIRI_backend.Dtos
{
    public class CentroCostoFiltroRequest
    {
        [Required]
        [StringLength(50)]
        public string Pfiltro_c { get; set; }
    }
}
