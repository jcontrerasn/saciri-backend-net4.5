using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Dtos;
using SACIRI_backend.Entities;
using SACIRI_backend.Interfaces;

namespace SACIRI_backend.Services
{
    public class UsuarioRolService
    {
        private readonly IUsuarioRolRepository repository;

        public UsuarioRolService(IUsuarioRolRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.repository = repository;
        }

        public async Task<UsuarioRolResponse> CrearAsync(
            CrearUsuarioRolRequest request,
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

            if (request.Usuario.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "El usuario no puede superar los 100 caracteres.",
                    "Usuario");
            }

            if (request.IdRol <= 0)
            {
                throw new ArgumentException(
                    "El identificador del rol debe ser mayor que cero.",
                    "IdRol");
            }

            if (!string.IsNullOrWhiteSpace(request.Jefe)
                && request.Jefe.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "El jefe no puede superar los 100 caracteres.",
                    "Jefe");
            }

            var usuario = request.Usuario.Trim();

            if (await repository.ExistePorUsuarioAsync(
                usuario, cancellationToken).ConfigureAwait(false))
            {
                throw new ArgumentException(
                    "El usuario ya está asignado a un Rol.");
            }

            var usuarioRol = await repository.CrearAsync(
                usuario,
                request.IdRol,
                NormalizarTexto(request.Jefe),
                cancellationToken).ConfigureAwait(false);

            return MapearRespuesta(usuarioRol);
        }

        public async Task<UsuarioRolResponse> ActualizarAsync(
            int idUsuarioRol,
            ActualizarUsuarioRolRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentException(
                    "El cuerpo de la solicitud es obligatorio.", "request");
            }

            if (idUsuarioRol <= 0)
            {
                throw new ArgumentException(
                    "El identificador del usuario-rol debe ser mayor que cero.",
                    "idUsuarioRol");
            }

            if (request.IdRol <= 0)
            {
                throw new ArgumentException(
                    "El identificador del rol debe ser mayor que cero.",
                    "IdRol");
            }

            if (!string.IsNullOrWhiteSpace(request.Jefe)
                && request.Jefe.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "El jefe no puede superar los 100 caracteres.",
                    "Jefe");
            }

            var usuarioRol = await repository.ActualizarAsync(
                idUsuarioRol,
                request.IdRol,
                NormalizarTexto(request.Jefe),
                cancellationToken).ConfigureAwait(false);

            return usuarioRol == null ? null : MapearRespuesta(usuarioRol);
        }

        public async Task<UsuarioRolResponse> CambiarEstadoAsync(
            int idUsuarioRol,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (idUsuarioRol <= 0)
            {
                throw new ArgumentException(
                    "El identificador del usuario-rol debe ser mayor que cero.",
                    "idUsuarioRol");
            }

            var usuarioRol = await repository.CambiarEstadoAsync(
                idUsuarioRol,
                cancellationToken).ConfigureAwait(false);

            return usuarioRol == null ? null : MapearRespuesta(usuarioRol);
        }

        public async Task<IList<UsuarioRolResponse>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var usuariosRoles = await repository.ObtenerTodosAsync(
                cancellationToken).ConfigureAwait(false);

            return usuariosRoles.Select(MapearRespuesta).ToList();
        }

        private static string NormalizarTexto(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private static UsuarioRolResponse MapearRespuesta(
            UsuarioRol usuarioRol)
        {
            return new UsuarioRolResponse
            {
                IdUsuarioRol = usuarioRol.IdUsuarioRol,
                Usuario = usuarioRol.Usuario,
                IdRol = usuarioRol.IdRol,
                Rol = usuarioRol.Rol == null ? null : usuarioRol.Rol.Nombre,
                Jefe = usuarioRol.Jefe,
                Activo = usuarioRol.Activo,
                FechaRegistro = usuarioRol.FechaRegistro
            };
        }
    }
}
