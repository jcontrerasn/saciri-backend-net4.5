using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface IPuestoRepository
    {
        Task<Puesto> CrearAsync(
            string nombre,
            string descripcion,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<IList<Puesto>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken));

        Task<Puesto> ActualizarAsync(
            int idPuesto,
            string nombre,
            string descripcion,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<Puesto> CambiarEstadoAsync(
            int idPuesto,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
