using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Dtos;
using SACIRI_backend.Interfaces;

namespace SACIRI_backend.Services
{
    public class TipoRegistroService
    {
        private readonly ITipoRegistroRepository repository;

        public TipoRegistroService(ITipoRegistroRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.repository = repository;
        }

        public async Task<IList<TipoRegistroResponse>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var tiposRegistro = await repository.ObtenerTodosAsync(
                cancellationToken).ConfigureAwait(false);

            return tiposRegistro.Select(tipoRegistro => new TipoRegistroResponse
            {
                IdTipoRegistro = tipoRegistro.IdTipoRegistro,
                Nombre = tipoRegistro.Nombre,
                Descripcion = tipoRegistro.Descripcion,
                Activo = tipoRegistro.Activo,
                FechaRegistro = tipoRegistro.FechaRegistro
            }).ToList();
        }
    }
}
