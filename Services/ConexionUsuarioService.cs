using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Dtos;
using SACIRI_backend.Interfaces;

namespace SACIRI_backend.Services
{
    public class ConexionUsuarioService
    {
        private readonly IConexionUsuarioRepository repository;

        public ConexionUsuarioService(IConexionUsuarioRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.repository = repository;
        }

        public async Task<ConexionUsuarioResponse> CrearAsync(
            RegistrarConexionUsuarioRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentException(
                    "El cuerpo de la solicitud es obligatorio.", "request");
            }

            if (string.IsNullOrWhiteSpace(request.Usuario))
            {
                throw new ArgumentException(
                    "El usuario es obligatorio.", "Usuario");
            }

            var usuario = request.Usuario.Trim();
            if (usuario.Length > 100)
            {
                throw new ArgumentException(
                    "Usuario no puede superar los 100 caracteres.",
                    "Usuario");
            }

            if (string.IsNullOrWhiteSpace(request.UsuarioAccion))
            {
                throw new ArgumentException(
                    "El usuario de acciÃ³n es obligatorio.",
                    "UsuarioAccion");
            }

            var usuarioAccion = request.UsuarioAccion.Trim();
            if (usuarioAccion.Length > 100)
            {
                throw new ArgumentException(
                    "UsuarioAccion no puede superar los 100 caracteres.",
                    "UsuarioAccion");
            }

            var sustento = string.IsNullOrWhiteSpace(request.Sustento)
                ? null
                : request.Sustento.Trim();

            if (sustento != null && sustento.Length > 500)
            {
                throw new ArgumentException(
                    "Sustento no puede superar los 500 caracteres.",
                    "Sustento");
            }

            var conexion = await repository.CrearAsync(
                usuario,
                usuarioAccion,
                sustento,
                cancellationToken).ConfigureAwait(false);

            if (conexion == null)
            {
                return null;
            }

            return new ConexionUsuarioResponse
            {
                IdConexion = conexion.IdConexion,
                Usuario = conexion.Usuario,
                Fecha = conexion.Fecha,
                Estado = conexion.Estado
            };
        }

        public async Task<EstadoConexionUsuarioResponse> ObtenerEstadoAsync(
            string usuario,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                throw new ArgumentException(
                    "El usuario es obligatorio.", "usuario");
            }

            usuario = usuario.Trim();
            if (usuario.Length > 100)
            {
                throw new ArgumentException(
                    "Usuario no puede superar los 100 caracteres.",
                    "usuario");
            }

            var conexion = await repository.ObtenerPorUsuarioAsync(
                usuario,
                cancellationToken).ConfigureAwait(false);

            return conexion == null
                ? null
                : new EstadoConexionUsuarioResponse
                {
                    Usuario = conexion.Usuario,
                    Estado = conexion.Estado
                };
        }

        public async Task<IList<ConexionUsuarioResponse>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var conexiones = await repository.ObtenerTodosAsync(
                cancellationToken).ConfigureAwait(false);

            return conexiones.Select(conexion => new ConexionUsuarioResponse
            {
                IdConexion = conexion.IdConexion,
                Usuario = conexion.Usuario,
                Fecha = conexion.Fecha,
                Estado = conexion.Estado
            }).ToList();
        }
    }
}
