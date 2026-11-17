using System;

namespace SACIRI_backend.Dtos
{
    public class SolicitudFlagResponse
    {
        public string CodigoSolicitud { get; set; }

        public string TipoRegistro { get; set; }

        public string Categoria { get; set; }

        public string CreadoPor { get; set; }

        public string SolicitadoPara { get; set; }

        public string AsignadoA { get; set; }

        public DateTime FechaSolicitud { get; set; }

        public DateTime? FechaEstimada { get; set; }

        public string CopiaJefatura { get; set; }

        public string Estado { get; set; }
    }
}
