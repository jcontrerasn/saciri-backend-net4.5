using System;

namespace SACIRI_backend.Dtos
{
    public class SolicitudComentarioResponse
    {
        public int IdComentario { get; set; }
        public string Comentario { get; set; }
        public string Tipo { get; set; }
        public string Usuario { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
