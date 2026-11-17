using System.ComponentModel.DataAnnotations;

namespace SACIRI_backend.Dtos
{
    public class SolicitudFlagRequest
    {
        [Required]
        [StringLength(100)]
        public string Usuario { get; set; }

        [Required]
        [StringLength(5)]
        public string Flag { get; set; }
    }
}
