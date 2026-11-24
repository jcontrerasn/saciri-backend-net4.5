using System;

namespace SACIRI_backend.Entities
{
    public class HistorialConexion
    {
        public int IdHistorialConexion { get; set; }
        public string Usuario { get; set; }
        public string UsuarioAccion { get; set; }
        public bool Estado { get; set; }
        public string Sustento { get; set; }
        public DateTime Fecha { get; set; }
    }
}
