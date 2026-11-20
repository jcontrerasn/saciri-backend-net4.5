using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface IHistorialSolicitudRepository
    {
        Task<IList<HistorialSolicitud>> ObtenerPorSolicitudAsync(
            int idSolicitud,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
