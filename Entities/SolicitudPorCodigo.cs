using System;
using System.Collections.Generic;

namespace SACIRI_backend.Entities
{
    public class SolicitudPorCodigo
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
        public IList<SolicitudComentarioDetalle> Comentarios { get; set; }
        public string CentroCosto { get; set; }
        public string NombreCentroCosto { get; set; }
        public string Lugar { get; set; }

        public SolicitudPorCodigo()
        {
            Comentarios = new List<SolicitudComentarioDetalle>();
        }
    }
}
