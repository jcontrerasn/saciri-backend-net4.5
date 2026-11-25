using System;

namespace SACIRI_backend.Dtos
{
    public class AdjuntoSolicitudResponse
    {
        public int IdAdjunto { get; set; }
        public int IdSolicitud { get; set; }
        public int IdHistorialSolicitud { get; set; }
        public int? IdComentario { get; set; }
        public string NombreAdjunto { get; set; }
        public string Formato { get; set; }
        public long TamanoAdjunto { get; set; }
        public string Ruta { get; set; }
        public string AdjuntadoPor { get; set; }
        public DateTime Fecha { get; set; }
        public bool Estado { get; set; }
    }
}
