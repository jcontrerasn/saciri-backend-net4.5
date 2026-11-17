using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface ISolicitudPorCodigoRepository
    {
        Task<SolicitudPorCodigo> BuscarAsync(
            string codigoSolicitud,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
