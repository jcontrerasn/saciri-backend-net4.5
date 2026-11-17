using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface IComentarioSolicitudRepository
    {
        Task<IList<ComentarioSolicitud>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
