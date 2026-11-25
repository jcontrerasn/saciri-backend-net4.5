using System;
using System.Collections.Generic;

namespace SACIRI_backend.Dtos
{
    public class SolicitudComentarioResponse
    {
        public int IdComentario { get; set; }
        public string Comentario { get; set; }
        public string Tipo { get; set; }
        public string Usuario { get; set; }
        public DateTime FechaRegistro { get; set; }
        public List<AdjuntoSolicitudResponse> Adjuntos { get; set; }

        public SolicitudComentarioResponse()
        {
            Adjuntos = new List<AdjuntoSolicitudResponse>();
        }
    }
}
