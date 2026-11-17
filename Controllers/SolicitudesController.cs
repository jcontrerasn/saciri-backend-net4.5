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
    [RoutePrefix("api/solicitudes")]
    public class SolicitudesController : ApiController
    {
        private readonly SolicitudService solicitudService;

        public SolicitudesController()
            : this(DependencyConfig.CreateSolicitudService())
        {
        }

        public SolicitudesController(SolicitudService solicitudService)
        {
            if (solicitudService == null)
            {
                throw new ArgumentNullException("solicitudService");
            }

            this.solicitudService = solicitudService;
        }

        // POST /api/solicitudes/flag
        [HttpPost]
        [Route("flag")]
        public async Task<IHttpActionResult> ObtenerFlag(
            [FromBody] SolicitudFlagRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentException(
                        "El cuerpo de la solicitud es obligatorio.", "request");
                }

                var solicitudes = await solicitudService.ObtenerFlagAsync(
                    request.Usuario,
                    request.Flag,
                    cancellationToken);

                return Ok(new ApiResponse<List<SolicitudFlagResponse>>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Solicitudes obtenidas correctamente por flag.",
                    Data = new List<SolicitudFlagResponse>(solicitudes)
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
