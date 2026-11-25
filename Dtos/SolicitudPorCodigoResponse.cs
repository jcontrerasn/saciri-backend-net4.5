using System;
using System.Collections.Generic;

namespace SACIRI_backend.Dtos
{
    public class SolicitudPorCodigoResponse
    {
        public int IdSolicitud { get; set; }
        public string CodigoSolicitud { get; set; }
        public string CreadoPor { get; set; }
        public string SolicitadoPara { get; set; }
        public string Ubicacion { get; set; }
        public string Area { get; set; }
        public string Telefono { get; set; }
        public string ContactoArea { get; set; }
        public string Correo { get; set; }
        public string TelefonoContactoArea { get; set; }
        public string TipoRegistro { get; set; }
        public string Estado { get; set; }
        public string Categoria { get; set; }
        public string Subcategoria { get; set; }
        public string Ocurrencia { get; set; }
        public string CopiaJefatura { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string AsignadoA { get; set; }
        public DateTime? FechaInicioAtencion { get; set; }
        public DateTime? FechaCaducidad { get; set; }
        public List<SolicitudComentarioResponse> Comentarios { get; set; }
        public List<AdjuntoSolicitudResponse> Adjuntos { get; set; }
        public List<string> RutasAdjuntos { get; set; }
        public string CentroCosto { get; set; }
        public string NombreCentroCosto { get; set; }
        public string Lugar { get; set; }

        public SolicitudPorCodigoResponse()
        {
            CodigoSolicitud = string.Empty;
            Comentarios = new List<SolicitudComentarioResponse>();
            Adjuntos = new List<AdjuntoSolicitudResponse>();
            RutasAdjuntos = new List<string>();
        }
    }
}
