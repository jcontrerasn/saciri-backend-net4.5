using System;

namespace SACIRI_backend.Dtos
{
    public class UsuarioRolResponse
    {
        public int IdUsuarioRol { get; set; }
        public string Usuario { get; set; }
        public int IdRol { get; set; }
        public string Rol { get; set; }
        public string Jefe { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
