using System;

namespace SACIRI_backend.Entities
{
    public class ConexionUsuario
    {
        public int IdConexion { get; set; }
        public string Usuario { get; set; }
        public DateTime Fecha { get; set; }
        public bool Estado { get; set; }
    }
}
