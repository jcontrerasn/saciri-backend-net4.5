using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface ISubcategoriaRepository
    {
        Task<Subcategoria> CrearAsync(
            int idCategoria,
            string nombre,
            string descripcion,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<IList<Subcategoria>> ObtenerTodasAsync(
            CancellationToken cancellationToken = default(CancellationToken));

        Task<IList<Subcategoria>> ObtenerPorCategoriaAsync(
            int idCategoria,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<Subcategoria> ActualizarAsync(
            int idSubcategoria,
            string nombre,
            string descripcion,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<Subcategoria> CambiarEstadoAsync(
            int idSubcategoria,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
