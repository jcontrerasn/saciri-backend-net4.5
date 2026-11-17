using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<IList<Categoria>> ObtenerPorTipoRegistroAsync(
            int idTipoRegistro,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
