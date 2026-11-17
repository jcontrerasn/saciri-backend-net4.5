using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface ISubcategoriaRepository
    {
        Task<IList<Subcategoria>> ObtenerPorCategoriaAsync(
            int idCategoria,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
