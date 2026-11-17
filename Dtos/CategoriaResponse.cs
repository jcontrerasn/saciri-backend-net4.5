using System;

namespace SACIRI_backend.Dtos
{
    public class CategoriaResponse
    {
        public int IdCategoria { get; set; }

        public string TipoRegistro { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}
