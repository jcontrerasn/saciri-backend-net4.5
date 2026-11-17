using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface IDetalleSolicitudRepository
    {
        Task<IList<DetalleSolicitud>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
