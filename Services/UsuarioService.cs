using System;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Dtos;
using SACIRI_backend.Interfaces;

namespace SACIRI_backend.Services
{
    public class UsuarioService
    {
        private readonly IUsuarioRepository repository;

        public UsuarioService(IUsuarioRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.repository = repository;
        }

        public async Task<UsuarioCuentaResponse> BuscarPorCuentaAsync(
            string cuenta,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(cuenta))
            {
                throw new ArgumentException(
                    "La cuenta es obligatoria.", "cuenta");
            }

            var usuario = await repository.BuscarPorCuentaAsync(
                cuenta.Trim(), cancellationToken).ConfigureAwait(false);

            if (usuario == null)
            {
                return null;
            }

            return new UsuarioCuentaResponse
            {
                Usuario = usuario.Cuenta,
                NombreCompleto = usuario.NombreCompleto,
                Area = usuario.Area
            };
        }
    }
}
