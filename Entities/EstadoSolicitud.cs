using System;

namespace SACIRI_backend.Entities
{
    public class EstadoSolicitud
    {
        public int IdEstadoSolicitud { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
