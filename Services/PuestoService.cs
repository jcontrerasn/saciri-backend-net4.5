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
    public class PuestoService
    {
        private readonly IPuestoRepository repository;

        public PuestoService(IPuestoRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.repository = repository;
        }

        public async Task<PuestoResponse> CrearAsync(
            CrearPuestoRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidarNombreYDescripcion(
                request == null ? null : request.Nombre,
                request == null ? null : request.Descripcion,
                request == null);

            var puesto = await repository.CrearAsync(
                request.Nombre.Trim(),
                NormalizarDescripcion(request.Descripcion),
                cancellationToken).ConfigureAwait(false);

            return MapearRespuesta(puesto);
        }

        public async Task<IList<PuestoResponse>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var puestos = await repository.ObtenerTodosAsync(
                cancellationToken).ConfigureAwait(false);

            return puestos.Select(MapearRespuesta).ToList();
        }

        public async Task<PuestoResponse> ActualizarAsync(
            int idPuesto,
            ActualizarPuestoRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (idPuesto <= 0)
            {
                throw new ArgumentException(
                    "El identificador del puesto debe ser mayor que cero.",
                    "idPuesto");
            }

            ValidarNombreYDescripcion(
                request == null ? null : request.Nombre,
                request == null ? null : request.Descripcion,
                request == null);

            var puesto = await repository.ActualizarAsync(
                idPuesto,
                request.Nombre.Trim(),
                NormalizarDescripcion(request.Descripcion),
                cancellationToken).ConfigureAwait(false);

            return puesto == null ? null : MapearRespuesta(puesto);
        }

        public async Task<PuestoResponse> CambiarEstadoAsync(
            int idPuesto,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (idPuesto <= 0)
            {
                throw new ArgumentException(
                    "El identificador del puesto debe ser mayor que cero.",
                    "idPuesto");
            }

            var puesto = await repository.CambiarEstadoAsync(
                idPuesto,
                cancellationToken).ConfigureAwait(false);

            return puesto == null ? null : MapearRespuesta(puesto);
        }

        private static void ValidarNombreYDescripcion(
            string nombre,
            string descripcion,
            bool cuerpoNulo)
        {
            if (cuerpoNulo)
            {
                throw new ArgumentException(
                    "El cuerpo de la solicitud es obligatorio.", "request");
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ArgumentException(
                    "El nombre es obligatorio.", "Nombre");
            }

            if (nombre.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "El nombre no puede superar los 100 caracteres.",
                    "Nombre");
            }

            if (!string.IsNullOrWhiteSpace(descripcion)
                && descripcion.Trim().Length > 250)
            {
                throw new ArgumentException(
                    "La descripciÃ³n no puede superar los 250 caracteres.",
                    "Descripcion");
            }
        }

        private static string NormalizarDescripcion(string descripcion)
        {
            return string.IsNullOrWhiteSpace(descripcion)
                ? null
                : descripcion.Trim();
        }

        private static PuestoResponse MapearRespuesta(Puesto puesto)
        {
            return new PuestoResponse
            {
                IdPuesto = puesto.IdPuesto,
                Nombre = puesto.Nombre,
                Descripcion = puesto.Descripcion,
                Activo = puesto.Activo,
                FechaRegistro = puesto.FechaRegistro
            };
        }
    }
}
