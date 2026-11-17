using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;
using System.Collections.Generic;

namespace SACIRI_backend.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario> BuscarPorCuentaAsync(
            string cuenta,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<IList<Usuario>> BuscarPorFiltroAsync(
            string filtro,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
