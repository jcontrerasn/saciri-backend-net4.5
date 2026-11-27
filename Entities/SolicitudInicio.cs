using System.Collections.Generic;

namespace SACIRI_backend.Entities
{
    public class SolicitudInicio
    {
        public IList<SolicitudFlag> PendientesEnvioInformacion {
            get; set;
        }

        public IList<SolicitudFlag> PendientesAprobacion { get; set; }

        public IList<SolicitudFlag> Asignadas { get; set; }
    }
}
