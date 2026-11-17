using System;

namespace SACIRI_backend.Entities
{
    public class UsuarioRol
    {
        public int IdUsuarioRol { get; set; }
        public string Usuario { get; set; }
        public int IdRol { get; set; }
        public Rol Rol { get; set; }
        public string Jefe { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
