using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Dtos;
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

            return categorias.Select(categoria => new CategoriaResponse
            {
                IdCategoria = categoria.IdCategoria,
                TipoRegistro = categoria.TipoRegistro,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Activo = categoria.Activo,
                FechaRegistro = categoria.FechaRegistro
            }).ToList();
        }
    }
}
