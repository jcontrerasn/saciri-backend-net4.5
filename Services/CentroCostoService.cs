using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Dtos;
using SACIRI_backend.Interfaces;

namespace SACIRI_backend.Services
{
    public class CentroCostoService
    {
        private readonly ICentroCostoRepository repository;

        public CentroCostoService(ICentroCostoRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.repository = repository;
        }

        public async Task<IList<CentroCostoResponse>> BuscarPorFiltroAsync(
            string filtro,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                throw new ArgumentException(
                    "El filtro es obligatorio.", "filtro");
            }

            var centrosCosto = await repository.BuscarPorFiltroAsync(
                filtro.Trim(), cancellationToken).ConfigureAwait(false);

            return centrosCosto.Select(centroCosto => new CentroCostoResponse
            {
                CentroCosto = centroCosto.CentroCostoCodigo,
                Area = centroCosto.Area,
                NombreCentroCosto = centroCosto.NombreCentroCosto,
                Activo = centroCosto.Activo
            }).ToList();
        }
    }
}
