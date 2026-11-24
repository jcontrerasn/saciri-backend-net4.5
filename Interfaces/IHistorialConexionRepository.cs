using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface IHistorialConexionRepository
    {
        Task<HistorialConexion> CrearAsync(
            HistorialConexion registro,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<IList<HistorialConexion>> ObtenerPorUsuarioAsync(
            string usuario,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
