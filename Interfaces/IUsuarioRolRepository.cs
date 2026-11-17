using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface IUsuarioRolRepository
    {
        Task<bool> ExistePorUsuarioAsync(
            string usuario,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<UsuarioRol> CrearAsync(
            string usuario,
            int idRol,
            string jefe,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<IList<UsuarioRol>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken));

        Task<UsuarioRol> ActualizarAsync(
            int idUsuarioRol,
            int idRol,
            string jefe,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<UsuarioRol> CambiarEstadoAsync(
            int idUsuarioRol,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
