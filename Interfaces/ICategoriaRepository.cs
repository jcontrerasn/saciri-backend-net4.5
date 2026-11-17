using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<Categoria> CrearAsync(
            int idTipoRegistro,
            string nombre,
            string descripcion,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<IList<Categoria>> ObtenerPorTipoRegistroAsync(
            int idTipoRegistro,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
