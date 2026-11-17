using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Dtos;
using SACIRI_backend.Interfaces;

namespace SACIRI_backend.Services
{
    public class SolicitudService
    {
        private readonly ISolicitudRepository repository;

        public SolicitudService(ISolicitudRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.repository = repository;
        }

        public async Task<IList<SolicitudFlagResponse>> ObtenerFlagAsync(
            string usuario,
            string flag,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                throw new ArgumentException(
                    "El usuario es obligatorio.", "usuario");
            }

            var flagNormalizado = (flag ?? string.Empty).Trim().ToUpperInvariant();

            if (flagNormalizado != "CS"
                && flagNormalizado != "A"
                && flagNormalizado != "TMS")
            {
                throw new ArgumentException(
                    "El flag debe ser CS, A o TMS.", "flag");
            }

            var solicitudes = await repository.ObtenerFlagAsync(
                usuario.Trim(),
                flagNormalizado,
                cancellationToken).ConfigureAwait(false);

            return solicitudes.Select(solicitud => new SolicitudFlagResponse
            {
                CodigoSolicitud = solicitud.CodigoSolicitud,
                TipoRegistro = solicitud.TipoRegistro,
                Categoria = solicitud.Categoria,
                CreadoPor = solicitud.CreadoPor,
                SolicitadoPara = solicitud.SolicitadoPara,
                AsignadoA = solicitud.AsignadoA,
                FechaSolicitud = solicitud.FechaSolicitud,
                FechaEstimada = solicitud.FechaEstimada,
                CopiaJefatura = solicitud.CopiaJefatura,
                Estado = solicitud.Estado
            }).ToList();
        }
    }
}
