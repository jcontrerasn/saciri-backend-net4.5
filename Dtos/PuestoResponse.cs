using System;

namespace SACIRI_backend.Dtos
{
    public class PuestoResponse
    {
        public int IdPuesto { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
