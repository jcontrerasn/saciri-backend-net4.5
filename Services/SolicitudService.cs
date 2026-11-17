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

        public async Task<RegistrarSolicitudResponse> RegistrarAsync(
            RegistrarSolicitudRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentException(
                    "El cuerpo de la solicitud es obligatorio.", "request");
            }

            if (string.IsNullOrWhiteSpace(request.CreadoPor))
            {
                throw new ArgumentException(
                    "El campo CreadoPor es obligatorio.", "CreadoPor");
            }

            if (string.IsNullOrWhiteSpace(request.CentroCosto))
            {
                throw new ArgumentException(
                    "El campo CentroCosto es obligatorio.", "CentroCosto");
            }

            if (string.IsNullOrWhiteSpace(request.Lugar))
            {
                throw new ArgumentException(
                    "El campo Lugar es obligatorio.", "Lugar");
            }

            if (string.IsNullOrWhiteSpace(request.Descripcion))
            {
                throw new ArgumentException(
                    "El campo Descripcion es obligatorio.", "Descripcion");
            }

            var codigoSolicitud = await repository.RegistrarAsync(
                request,
                cancellationToken).ConfigureAwait(false);

            return new RegistrarSolicitudResponse
            {
                CodigoSolicitud = codigoSolicitud
            };
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
