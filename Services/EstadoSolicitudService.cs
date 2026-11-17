using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Dtos;
using SACIRI_backend.Interfaces;

namespace SACIRI_backend.Services
{
    public class EstadoSolicitudService
    {
        private readonly IEstadoSolicitudRepository repository;

        public EstadoSolicitudService(IEstadoSolicitudRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.repository = repository;
        }

        public async Task<IList<EstadoSolicitudResponse>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var estados = await repository.ObtenerTodosAsync(
                cancellationToken).ConfigureAwait(false);

            return estados.Select(estado => new EstadoSolicitudResponse
            {
                IdEstadoSolicitud = estado.IdEstadoSolicitud,
                Nombre = estado.Nombre,
                Activo = estado.Activo,
                FechaRegistro = estado.FechaRegistro
            }).ToList();
        }
    }
}
