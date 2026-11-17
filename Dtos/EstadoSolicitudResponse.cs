using System;

namespace SACIRI_backend.Dtos
{
    public class EstadoSolicitudResponse
    {
        public int IdEstadoSolicitud { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
