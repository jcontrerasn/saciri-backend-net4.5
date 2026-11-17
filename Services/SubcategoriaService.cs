using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Dtos;
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

            return subcategorias.Select(subcategoria => new SubcategoriaResponse
            {
                IdSubcategoria = subcategoria.IdSubcategoria,
                IdCategoria = subcategoria.IdCategoria,
                Nombre = subcategoria.Nombre,
                Descripcion = subcategoria.Descripcion,
                Activo = subcategoria.Activo,
                FechaRegistro = subcategoria.FechaRegistro
            }).ToList();
        }
    }
}
