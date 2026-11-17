using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface ISolicitudRepository
    {
        Task<IList<SolicitudFlag>> ObtenerFlagAsync(
            string usuario,
            string flag,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
