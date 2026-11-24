using System;

namespace SACIRI_backend.Dtos
{
    public class ConexionUsuarioResponse
    {
        public int IdConexion { get; set; }
        public string Usuario { get; set; }
        public DateTime Fecha { get; set; }
        public bool Estado { get; set; }
    }
}
