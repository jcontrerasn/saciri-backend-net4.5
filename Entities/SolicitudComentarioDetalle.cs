using System;
using System.Collections.Generic;

namespace SACIRI_backend.Entities
{
    public class SolicitudComentarioDetalle
    {
        public int IdComentario { get; set; }
        public string Comentario { get; set; }
        public string Tipo { get; set; }
        public string Usuario { get; set; }
        public DateTime FechaRegistro { get; set; }
        public IList<AdjuntoSolicitud> Adjuntos { get; set; }

        public SolicitudComentarioDetalle()
        {
            Adjuntos = new List<AdjuntoSolicitud>();
        }
    }
}
