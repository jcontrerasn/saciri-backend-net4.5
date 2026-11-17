using System;

namespace SACIRI_backend.Entities
{
    public class SolicitudComentarioDetalle
    {
        public int IdComentario { get; set; }
        public string Comentario { get; set; }
        public string Tipo { get; set; }
        public string Usuario { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
