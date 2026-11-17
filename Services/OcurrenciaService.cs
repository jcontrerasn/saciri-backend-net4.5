using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Dtos;
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

            return ocurrencias.Select(ocurrencia => new OcurrenciaResponse
            {
                IdOcurrencia = ocurrencia.IdOcurrencia,
                IdSubcategoria = ocurrencia.IdSubcategoria,
                Nombre = ocurrencia.Nombre,
                Descripcion = ocurrencia.Descripcion,
                Activo = ocurrencia.Activo,
                FechaRegistro = ocurrencia.FechaRegistro
            }).ToList();
        }
    }
}
