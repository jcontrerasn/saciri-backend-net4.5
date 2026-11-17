using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface ICentroCostoRepository
    {
        Task<IList<CentroCosto>> BuscarPorFiltroAsync(
            string filtro,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
