using System;

namespace SACIRI_backend.Entities
{
    public class TipoRegistro
    {
        public int IdTipoRegistro { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}
