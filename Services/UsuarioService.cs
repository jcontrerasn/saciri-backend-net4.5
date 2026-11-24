using System;
using System.Collections.Generic;
using System.Linq;
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
                Rol = usuario.Rol,
                Area = usuario.Area
            };
        }

        public async Task<IList<UsuarioFiltroResponse>> BuscarPorFiltroAsync(
            string filtro,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                throw new ArgumentException(
                    "El filtro es obligatorio.", "filtro");
            }

            var usuarios = await repository.BuscarPorFiltroAsync(
                filtro.Trim(), cancellationToken).ConfigureAwait(false);

            return usuarios.Select(usuario => new UsuarioFiltroResponse
            {
                Usuario = usuario.Cuenta,
                NombreCompleto = usuario.NombreCompleto,
                Ubicacion = usuario.Ubicacion,
                Area = usuario.Area,
                CentroCosto = usuario.CentroCosto,
                Telefono = usuario.Telefono,
                Correo = usuario.Correo,
                ContactoArea = usuario.ContactoArea,
                TelefonoContactoArea = usuario.TelefonoContactoArea
            }).ToList();
        }
    }
}
