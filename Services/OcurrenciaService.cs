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
    public class OcurrenciaService
    {
        private readonly IOcurrenciaRepository repository;

        public OcurrenciaService(IOcurrenciaRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.repository = repository;
        }

        public async Task<OcurrenciaResponse> CrearAsync(
            CrearOcurrenciaRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentException(
                    "El cuerpo de la solicitud es obligatorio.", "request");
            }

            if (request.IdSubcategoria <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la subcategoría debe ser mayor que cero.",
                    "IdSubcategoria");
            }

            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                throw new ArgumentException(
                    "El nombre es obligatorio.", "Nombre");
            }

            var ocurrencia = await repository.CrearAsync(
                request.IdSubcategoria,
                request.Nombre.Trim(),
                string.IsNullOrWhiteSpace(request.Descripcion)
                    ? null
                    : request.Descripcion.Trim(),
                cancellationToken).ConfigureAwait(false);

            return MapearRespuesta(ocurrencia);
        }

        public async Task<IList<OcurrenciaResponse>> ObtenerTodasAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var ocurrencias = await repository.ObtenerTodasAsync(
                cancellationToken).ConfigureAwait(false);

            return ocurrencias.Select(MapearRespuesta).ToList();
        }

        public async Task<OcurrenciaResponse> ActualizarAsync(
            int idOcurrencia,
            ActualizarOcurrenciaRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentException(
                    "El cuerpo de la solicitud es obligatorio.", "request");
            }

            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                throw new ArgumentException(
                    "El nombre es obligatorio.", "Nombre");
            }

            var ocurrencia = await repository.ActualizarAsync(
                idOcurrencia,
                request.Nombre.Trim(),
                string.IsNullOrWhiteSpace(request.Descripcion)
                    ? null
                    : request.Descripcion.Trim(),
                cancellationToken).ConfigureAwait(false);

            return ocurrencia == null ? null : MapearRespuesta(ocurrencia);
        }

        public async Task<OcurrenciaResponse> CambiarEstadoAsync(
            int idOcurrencia,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var ocurrencia = await repository.CambiarEstadoAsync(
                idOcurrencia,
                cancellationToken).ConfigureAwait(false);

            return ocurrencia == null ? null : MapearRespuesta(ocurrencia);
        }

        public async Task<IList<OcurrenciaResponse>> ObtenerPorSubcategoriaAsync(
            int idSubcategoria,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (idSubcategoria <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la subcategoría debe ser mayor que cero.",
                    "idSubcategoria");
            }

            var ocurrencias = await repository.ObtenerPorSubcategoriaAsync(
                idSubcategoria,
                cancellationToken).ConfigureAwait(false);

            return ocurrencias.Select(MapearRespuesta).ToList();
        }

        private static OcurrenciaResponse MapearRespuesta(
            Ocurrencia ocurrencia)
        {
            return new OcurrenciaResponse
            {
                IdOcurrencia = ocurrencia.IdOcurrencia,
                IdSubcategoria = ocurrencia.IdSubcategoria,
                Nombre = ocurrencia.Nombre,
                Descripcion = ocurrencia.Descripcion,
                Activo = ocurrencia.Activo,
                FechaRegistro = ocurrencia.FechaRegistro
            };
        }
    }
}
