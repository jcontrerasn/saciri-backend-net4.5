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
        private readonly IPuestoRepository puestoRepository;

        public UsuarioRolService(
            IUsuarioRolRepository repository,
            IPuestoRepository puestoRepository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            if (puestoRepository == null)
            {
                throw new ArgumentNullException("puestoRepository");
            }

            this.repository = repository;
            this.puestoRepository = puestoRepository;
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
                NormalizarTexto(request.Puestos),
                cancellationToken).ConfigureAwait(false);

            return await MapearRespuestaAsync(
                usuarioRol,
                cancellationToken).ConfigureAwait(false);
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
                NormalizarTexto(request.Puestos),
                cancellationToken).ConfigureAwait(false);

            return usuarioRol == null
                ? null
                : await MapearRespuestaAsync(
                    usuarioRol,
                    cancellationToken).ConfigureAwait(false);
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

            return usuarioRol == null
                ? null
                : await MapearRespuestaAsync(
                    usuarioRol,
                    cancellationToken).ConfigureAwait(false);
        }

        public async Task<IList<UsuarioRolResponse>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var usuariosRoles = await repository.ObtenerTodosAsync(
                cancellationToken).ConfigureAwait(false);

            var puestosPorId = await ObtenerPuestosPorIdAsync(
                cancellationToken).ConfigureAwait(false);

            return usuariosRoles.Select(usuarioRol =>
                MapearRespuesta(usuarioRol, puestosPorId)).ToList();
        }

        public async Task<IList<UsuarioRolResponse>> BuscarPorUsuarioAsync(
            string filtro,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                throw new ArgumentException(
                    "El filtro de usuario es obligatorio.", "filtro");
            }

            filtro = filtro.Trim();
            if (filtro.Length > 100)
            {
                throw new ArgumentException(
                    "El filtro no puede superar los 100 caracteres.",
                    "filtro");
            }

            var usuariosRoles = await repository.BuscarPorUsuarioAsync(
                filtro,
                cancellationToken).ConfigureAwait(false);

            var puestosPorId = await ObtenerPuestosPorIdAsync(
                cancellationToken).ConfigureAwait(false);

            return usuariosRoles.Select(usuarioRol =>
                MapearRespuesta(usuarioRol, puestosPorId)).ToList();
        }

        private static string NormalizarTexto(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private static UsuarioRolResponse MapearRespuesta(
            UsuarioRol usuarioRol,
            IDictionary<int, string> puestosPorId = null)
        {
            return new UsuarioRolResponse
            {
                IdUsuarioRol = usuarioRol.IdUsuarioRol,
                Usuario = usuarioRol.Usuario,
                IdRol = usuarioRol.IdRol,
                Rol = usuarioRol.Rol == null ? null : usuarioRol.Rol.Nombre,
                Jefe = usuarioRol.Jefe,
                Puestos = puestosPorId == null
                    ? new List<PuestoUsuarioRolResponse>()
                    : MapearPuestos(usuarioRol.Puestos, puestosPorId),
                EstadoConexion = usuarioRol.EstadoConexion,
                Activo = usuarioRol.Activo,
                FechaRegistro = usuarioRol.FechaRegistro
            };
        }

        private async Task<UsuarioRolResponse> MapearRespuestaAsync(
            UsuarioRol usuarioRol,
            CancellationToken cancellationToken)
        {
            var puestosPorId = await ObtenerPuestosPorIdAsync(
                cancellationToken).ConfigureAwait(false);

            return MapearRespuesta(usuarioRol, puestosPorId);
        }

        private async Task<IDictionary<int, string>> ObtenerPuestosPorIdAsync(
            CancellationToken cancellationToken)
        {
            var puestos = await puestoRepository.ObtenerTodosAsync(
                cancellationToken).ConfigureAwait(false);

            return puestos.ToDictionary(
                puesto => puesto.IdPuesto,
                puesto => puesto.Nombre);
        }

        private static IList<PuestoUsuarioRolResponse> MapearPuestos(
            string puestos,
            IDictionary<int, string> puestosPorId)
        {
            var respuesta = new List<PuestoUsuarioRolResponse>();

            if (string.IsNullOrWhiteSpace(puestos))
            {
                return respuesta;
            }

            foreach (var valor in puestos.Split(
                new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int idPuesto;
                if (!int.TryParse(valor.Trim(), out idPuesto))
                {
                    continue;
                }

                string nombre;
                puestosPorId.TryGetValue(idPuesto, out nombre);
                respuesta.Add(new PuestoUsuarioRolResponse
                {
                    IdPuesto = idPuesto,
                    Nombre = nombre
                });
            }

            return respuesta;
        }
    }
}
