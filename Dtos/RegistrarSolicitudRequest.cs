using System.ComponentModel.DataAnnotations;

namespace SACIRI_backend.Dtos
{
    public class RegistrarSolicitudRequest
    {
        [StringLength(100)]
        public string SolicitadoPara { get; set; }

        [Required]
        [StringLength(100)]
        public string CreadoPor { get; set; }

        [Required]
        public int IdTipoRegistro { get; set; }

        [Required]
        public int IdCategoria { get; set; }

        public int? IdSubcategoria { get; set; }
        public int? IdOcurrencia { get; set; }

        [Required]
        [StringLength(50)]
        public string CentroCosto { get; set; }

        [Required]
        [StringLength(200)]
        public string Lugar { get; set; }

        [Required]
        public string Descripcion { get; set; }

        public string Recomendacion { get; set; }
    }
}
