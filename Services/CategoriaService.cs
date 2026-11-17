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
    public class CategoriaService
    {
        private readonly ICategoriaRepository repository;

        public CategoriaService(ICategoriaRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.repository = repository;
        }

        public async Task<CategoriaResponse> CrearAsync(
            CrearCategoriaRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentException(
                    "El cuerpo de la solicitud es obligatorio.", "request");
            }

            if (request.IdTipoRegistro <= 0)
            {
                throw new ArgumentException(
                    "El identificador del tipo de registro debe ser mayor que cero.",
                    "IdTipoRegistro");
            }

            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                throw new ArgumentException(
                    "El nombre es obligatorio.", "Nombre");
            }

            var categoria = await repository.CrearAsync(
                request.IdTipoRegistro,
                request.Nombre.Trim(),
                string.IsNullOrWhiteSpace(request.Descripcion)
                    ? null
                    : request.Descripcion.Trim(),
                cancellationToken).ConfigureAwait(false);

            return MapearRespuesta(categoria);
        }

        public async Task<IList<CategoriaResponse>> ObtenerPorTipoRegistroAsync(
            int idTipoRegistro,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (idTipoRegistro <= 0)
            {
                throw new ArgumentException(
                    "El identificador del tipo de registro debe ser mayor que cero.",
                    "idTipoRegistro");
            }

            var categorias = await repository.ObtenerPorTipoRegistroAsync(
                idTipoRegistro,
                cancellationToken).ConfigureAwait(false);

            return categorias.Select(MapearRespuesta).ToList();
        }

        public async Task<IList<CategoriaResponse>> ObtenerTodasAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var categorias = await repository.ObtenerTodasAsync(
                cancellationToken).ConfigureAwait(false);

            return categorias.Select(MapearRespuesta).ToList();
        }

        public async Task<CategoriaResponse> ActualizarAsync(
            int idCategoria,
            ActualizarCategoriaRequest request,
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

            var categoria = await repository.ActualizarAsync(
                idCategoria,
                request.Nombre.Trim(),
                string.IsNullOrWhiteSpace(request.Descripcion)
                    ? null
                    : request.Descripcion.Trim(),
                cancellationToken).ConfigureAwait(false);

            return categoria == null ? null : MapearRespuesta(categoria);
        }

        public async Task<CategoriaResponse> CambiarEstadoAsync(
            int idCategoria,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var categoria = await repository.CambiarEstadoAsync(
                idCategoria,
                cancellationToken).ConfigureAwait(false);

            return categoria == null ? null : MapearRespuesta(categoria);
        }

        private static CategoriaResponse MapearRespuesta(Categoria categoria)
        {
            return new CategoriaResponse
            {
                IdCategoria = categoria.IdCategoria,
                TipoRegistro = categoria.TipoRegistro,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Activo = categoria.Activo,
                FechaRegistro = categoria.FechaRegistro
            };
        }
    }
}
