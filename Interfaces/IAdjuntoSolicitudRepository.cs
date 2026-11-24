using System.Threading;
using System.Threading.Tasks;
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
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
