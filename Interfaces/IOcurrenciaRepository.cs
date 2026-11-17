using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface IOcurrenciaRepository
    {
        Task<Ocurrencia> CrearAsync(
            int idSubcategoria,
            string nombre,
            string descripcion,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<IList<Ocurrencia>> ObtenerTodasAsync(
            CancellationToken cancellationToken = default(CancellationToken));

        Task<IList<Ocurrencia>> ObtenerPorSubcategoriaAsync(
            int idSubcategoria,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<Ocurrencia> ActualizarAsync(
            int idOcurrencia,
            string nombre,
            string descripcion,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<Ocurrencia> CambiarEstadoAsync(
            int idOcurrencia,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
