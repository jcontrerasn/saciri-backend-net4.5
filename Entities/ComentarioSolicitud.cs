using System;

namespace SACIRI_backend.Entities
{
    public class ComentarioSolicitud
    {
        public int IdComentario { get; set; }
        public int IdSolicitud { get; set; }
        public string Comentario { get; set; }
        public string Tipo { get; set; }
        public string Usuario { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
