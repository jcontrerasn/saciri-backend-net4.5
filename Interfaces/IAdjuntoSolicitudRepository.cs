using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface IAdjuntoSolicitudRepository
    {
        Task<AdjuntoSolicitud> CrearAsync(
            int idSolicitud,
            string nombreAdjunto,
            string formato,
            long tamanoAdjunto,
            string ruta,
            string adjuntadoPor,
            CancellationToken cancellationToken = default(CancellationToken),
            bool primerHistorial = false,
            int? idComentario = null);

        Task<IList<AdjuntoSolicitud>> ObtenerPorSolicitudAsync(
            int idSolicitud,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<AdjuntoSolicitud> ObtenerPorIdAsync(
            int idAdjunto,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
