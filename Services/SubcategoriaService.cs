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
    public class SubcategoriaService
    {
        private readonly ISubcategoriaRepository repository;

        public SubcategoriaService(ISubcategoriaRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.repository = repository;
        }

        public async Task<SubcategoriaResponse> CrearAsync(
            CrearSubcategoriaRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentException(
                    "El cuerpo de la solicitud es obligatorio.", "request");
            }

            if (request.IdCategoria <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la categoría debe ser mayor que cero.",
                    "IdCategoria");
            }

            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                throw new ArgumentException(
                    "El nombre es obligatorio.", "Nombre");
            }

            var subcategoria = await repository.CrearAsync(
                request.IdCategoria,
                request.Nombre.Trim(),
                string.IsNullOrWhiteSpace(request.Descripcion)
                    ? null
                    : request.Descripcion.Trim(),
                cancellationToken).ConfigureAwait(false);

            return MapearRespuesta(subcategoria);
        }

        public async Task<IList<SubcategoriaResponse>> ObtenerTodasAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var subcategorias = await repository.ObtenerTodasAsync(
                cancellationToken).ConfigureAwait(false);

            return subcategorias.Select(MapearRespuesta).ToList();
        }

        public async Task<SubcategoriaResponse> ActualizarAsync(
            int idSubcategoria,
            ActualizarSubcategoriaRequest request,
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

            var subcategoria = await repository.ActualizarAsync(
                idSubcategoria,
                request.Nombre.Trim(),
                string.IsNullOrWhiteSpace(request.Descripcion)
                    ? null
                    : request.Descripcion.Trim(),
                cancellationToken).ConfigureAwait(false);

            return subcategoria == null ? null : MapearRespuesta(subcategoria);
        }

        public async Task<SubcategoriaResponse> CambiarEstadoAsync(
            int idSubcategoria,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var subcategoria = await repository.CambiarEstadoAsync(
                idSubcategoria,
                cancellationToken).ConfigureAwait(false);

            return subcategoria == null ? null : MapearRespuesta(subcategoria);
        }

        public async Task<IList<SubcategoriaResponse>> ObtenerPorCategoriaAsync(
            int idCategoria,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (idCategoria <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la categoría debe ser mayor que cero.",
                    "idCategoria");
            }

            var subcategorias = await repository.ObtenerPorCategoriaAsync(
                idCategoria,
                cancellationToken).ConfigureAwait(false);

            return subcategorias.Select(MapearRespuesta).ToList();
        }

        private static SubcategoriaResponse MapearRespuesta(
            Subcategoria subcategoria)
        {
            return new SubcategoriaResponse
            {
                IdSubcategoria = subcategoria.IdSubcategoria,
                IdCategoria = subcategoria.IdCategoria,
                Nombre = subcategoria.Nombre,
                Descripcion = subcategoria.Descripcion,
                Activo = subcategoria.Activo,
                FechaRegistro = subcategoria.FechaRegistro
            };
        }
    }
}
