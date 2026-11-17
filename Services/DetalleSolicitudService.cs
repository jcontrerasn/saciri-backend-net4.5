using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Dtos;
using SACIRI_backend.Interfaces;

namespace SACIRI_backend.Services
{
    public class DetalleSolicitudService
    {
        private readonly IDetalleSolicitudRepository repository;

        public DetalleSolicitudService(IDetalleSolicitudRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.repository = repository;
        }

        public async Task<IList<DetalleSolicitudResponse>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var detalles = await repository.ObtenerTodosAsync(
                cancellationToken).ConfigureAwait(false);

            return detalles.Select(detalle => new DetalleSolicitudResponse
            {
                IdDetalleSolicitud = detalle.IdDetalleSolicitud,
                IdSolicitud = detalle.IdSolicitud,
                CentroCosto = detalle.CentroCosto,
                Lugar = detalle.Lugar
            }).ToList();
        }
    }
}
