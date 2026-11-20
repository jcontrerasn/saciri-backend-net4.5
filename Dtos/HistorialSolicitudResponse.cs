using System;

namespace SACIRI_backend.Dtos
{
    public class HistorialSolicitudResponse
    {
        public int IdHistorialSolicitud { get; set; }
        public int IdSolicitud { get; set; }
        public string SolicitadoPara { get; set; }
        public string CreadoPor { get; set; }
        public string CodigoSolicitud { get; set; }
        public int IdEstadoSolicitud { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public int IdTipoRegistro { get; set; }
        public int IdCategoria { get; set; }
        public int? IdSubcategoria { get; set; }
        public int? IdOcurrencia { get; set; }
        public string Prioridad { get; set; }
        public string AsignadoA { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaCaducidad { get; set; }
        public DateTime? FechaFinAtencion { get; set; }
        public int? DuracionAtencion { get; set; }
        public string CopiaJefatura { get; set; }
        public string JefeAprobador { get; set; }
        public string EstadoAprobacion { get; set; }
        public string UsuarioAprobacion { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public int? Nivel { get; set; }
        public string ProblemaMasivo { get; set; }
        public string CCSolicitadoPara { get; set; }
        public string Accion { get; set; }
        public string UsuarioModificacion { get; set; }
        public string Observacion { get; set; }
        public int? IdEstadoAnterior { get; set; }
        public int IdEstadoNuevo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
