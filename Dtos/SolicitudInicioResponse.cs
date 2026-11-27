using System.Collections.Generic;
using Newtonsoft.Json;

namespace SACIRI_backend.Dtos
{
    public class SolicitudInicioResponse
    {
        [JsonProperty("PENDIENTES_ENVIO_INFORMACION")]
        public SolicitudFlagGroupResponse PendientesEnvioInformacion {
            get; set;
        }

        [JsonProperty("PENDIENTES_APROBACION")]
        public SolicitudFlagGroupResponse PendientesAprobacion {
            get; set;
        }

        [JsonProperty("ASIGNADAS")]
        public SolicitudFlagGroupResponse Asignadas { get; set; }
    }

    public class SolicitudFlagGroupResponse
    {
        public int Total { get; set; }

        public IList<SolicitudFlagResponse> Solicitudes { get; set; }
    }
}
