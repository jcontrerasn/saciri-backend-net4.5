using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface IOcurrenciaRepository
    {
        Task<IList<Ocurrencia>> ObtenerPorSubcategoriaAsync(
            int idSubcategoria,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
