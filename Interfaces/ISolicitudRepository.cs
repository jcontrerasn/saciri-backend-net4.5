using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Dtos;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface ISolicitudRepository
    {
        Task<RegistrarSolicitudResponse> RegistrarAsync(
            RegistrarSolicitudRequest request,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<IList<SolicitudFlag>> ObtenerFlagAsync(
            string usuario,
            string flag,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<SolicitudEnProcesoResponse> PasarAEnProcesoAsync(
            string codigoSolicitud,
            string usuario,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<SolicitudEnProcesoResponse> SolicitarInformacionAsync(
            int idSolicitud,
            string comentario,
            string usuario,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<SolicitudEnProcesoResponse> EnviarInformacionAsync(
            int idSolicitud,
            string comentario,
            string usuario,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<SolicitudEnProcesoResponse> AnularSolicitudAsync(
            int idSolicitud,
            string comentario,
            string usuario,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<SolicitudEnProcesoResponse> FinalizarSolicitudAsync(
            int idSolicitud,
            string comentario,
            string usuario,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<SolicitudEnProcesoResponse> ReasignarSolicitudAsync(
            int idSolicitud,
            string comentario,
            string usuario,
            string asignadoA,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<SolicitudEnProcesoResponse> DerivarSolicitudAsync(
            int idSolicitud,
            string comentario,
            string usuario,
            string asignadoA,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
