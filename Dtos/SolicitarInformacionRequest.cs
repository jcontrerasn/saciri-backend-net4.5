using System.ComponentModel.DataAnnotations;

namespace SACIRI_backend.Dtos
{
    public class SolicitarInformacionRequest
    {
        [Range(1, int.MaxValue)]
        public int IdSolicitud { get; set; }

        [Required]
        public string Comentario { get; set; }

        [Required]
        [StringLength(100)]
        public string Usuario { get; set; }
    }
}
