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
    public class HistorialConexionService
    {
        private readonly IHistorialConexionRepository repository;

        public HistorialConexionService(IHistorialConexionRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.repository = repository;
        }

        public async Task<HistorialConexionResponse> CrearAsync(
            RegistrarHistorialConexionRequest request,
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

            if (!request.Estado.HasValue)
            {
                throw new ArgumentException(
                    "El estado es obligatorio.", "Estado");
            }

            if (string.IsNullOrWhiteSpace(request.UsuarioAccion))
            {
                throw new ArgumentException(
                    "El usuario de acciÃ³n es obligatorio.",
                    "UsuarioAccion");
            }

            var usuario = request.Usuario.Trim();
            if (usuario.Length > 100)
            {
                throw new ArgumentException(
                    "Usuario no puede superar los 100 caracteres.",
                    "Usuario");
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

            var registro = await repository.CrearAsync(
                new HistorialConexion
                {
                    Usuario = usuario,
                    UsuarioAccion = usuarioAccion,
                    Estado = request.Estado.Value,
                    Sustento = sustento
                },
                cancellationToken).ConfigureAwait(false);

            return new HistorialConexionResponse
            {
                IdHistorialConexion = registro.IdHistorialConexion,
                Usuario = registro.Usuario,
                UsuarioAccion = registro.UsuarioAccion,
                Estado = registro.Estado,
                Sustento = registro.Sustento,
                Fecha = registro.Fecha
            };
        }

        public async Task<IList<HistorialConexionResponse>> ObtenerPorUsuarioAsync(
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

            var registros = await repository.ObtenerPorUsuarioAsync(
                usuario,
                cancellationToken).ConfigureAwait(false);

            return registros.Select(MapearRespuesta).ToList();
        }

        private static HistorialConexionResponse MapearRespuesta(
            HistorialConexion registro)
        {
            return new HistorialConexionResponse
            {
                IdHistorialConexion = registro.IdHistorialConexion,
                Usuario = registro.Usuario,
                UsuarioAccion = registro.UsuarioAccion,
                Estado = registro.Estado,
                Sustento = registro.Sustento,
                Fecha = registro.Fecha
            };
        }
    }
}
