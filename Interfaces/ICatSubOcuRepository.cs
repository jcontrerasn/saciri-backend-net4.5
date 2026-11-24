using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface ICatSubOcuRepository
    {
        Task<CatSubOcu> CrearAsync(
            CatSubOcu registro,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<CatSubOcu> ActualizarAsync(
            int id,
            CatSubOcu registro,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<CatSubOcu> ObtenerPorIdAsync(
            int id,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<IList<CatSubOcu>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken));

        Task<IList<CatSubOcu>> ObtenerPorOcurrenciaAsync(
            int idOcurrencia,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
