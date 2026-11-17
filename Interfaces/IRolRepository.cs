using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface IRolRepository
    {
        Task<IList<Rol>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
