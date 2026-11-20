using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using SACIRI_backend.Dtos;
using SACIRI_backend.Services;

namespace SACIRI_backend.Controllers
{
    [RoutePrefix("api/historial-solicitudes")]
    public class HistorialSolicitudesController : ApiController
    {
        private readonly HistorialSolicitudService historialSolicitudService;

        public HistorialSolicitudesController()
            : this(DependencyConfig.CreateHistorialSolicitudService())
        {
        }

        public HistorialSolicitudesController(
            HistorialSolicitudService historialSolicitudService)
        {
            if (historialSolicitudService == null)
            {
                throw new ArgumentNullException("historialSolicitudService");
            }

            this.historialSolicitudService = historialSolicitudService;
        }

        // GET /api/historial-solicitudes/{idSolicitud}
        [HttpGet]
        [Route("{idSolicitud:int}")]
        public async Task<IHttpActionResult> ObtenerPorSolicitud(
            int idSolicitud,
            CancellationToken cancellationToken)
        {
            try
            {
                var historial = await historialSolicitudService
                    .ObtenerPorSolicitudAsync(
                        idSolicitud,
                        cancellationToken);

                return Ok(new ApiResponse<List<HistorialSolicitudResponse>>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Historial de solicitud obtenido correctamente.",
                    Data = new List<HistorialSolicitudResponse>(historial)
                });
            }
            catch (ArgumentException exception)
            {
                return Content(
                    HttpStatusCode.BadRequest,
                    new ApiResponse<object>
                    {
                        Status = (int)HttpStatusCode.BadRequest,
                        Message = exception.Message,
                        Data = null
                    });
            }
        }
    }
}
