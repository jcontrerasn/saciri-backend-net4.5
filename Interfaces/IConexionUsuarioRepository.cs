using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface IConexionUsuarioRepository
    {
        Task<ConexionUsuario> CrearAsync(
            string usuario,
            string usuarioAccion,
            string sustento,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<ConexionUsuario> ObtenerPorUsuarioAsync(
            string usuario,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<IList<ConexionUsuario>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
