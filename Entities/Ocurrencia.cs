using System;

namespace SACIRI_backend.Entities
{
    public class Ocurrencia
    {
        public int IdOcurrencia { get; set; }

        public int IdSubcategoria { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}
