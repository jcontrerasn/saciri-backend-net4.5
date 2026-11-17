using System;
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
        public int IdEstadoSolicitud { get; set; }

        [Required]
        public int IdTipoRegistro { get; set; }

        [Required]
        public int IdCategoria { get; set; }

        public int? IdSubcategoria { get; set; }
        public int? IdOcurrencia { get; set; }

        [StringLength(50)]
        public string Prioridad { get; set; }

        [StringLength(100)]
        public string AsignadoA { get; set; }

        public DateTime? FechaSolicitud { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaCaducidad { get; set; }
        public DateTime? FechaFinAtencion { get; set; }
        public int? DuracionAtencion { get; set; }

        [StringLength(100)]
        public string CopiaJefatura { get; set; }

        [StringLength(100)]
        public string JefeAprobador { get; set; }

        [StringLength(100)]
        public string EstadoAprobacion { get; set; }

        [StringLength(100)]
        public string UsuarioAprobacion { get; set; }

        public DateTime? FechaAprobacion { get; set; }
        public int? Nivel { get; set; }

        [StringLength(2)]
        public string ProblemaMasivo { get; set; }

        [StringLength(100)]
        public string CCSolicitadoPara { get; set; }

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
