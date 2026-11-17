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
    [RoutePrefix("api/detalle-solicitudes")]
    public class DetalleSolicitudesController : ApiController
    {
        private readonly DetalleSolicitudService detalleSolicitudService;

        public DetalleSolicitudesController()
            : this(DependencyConfig.CreateDetalleSolicitudService())
        {
        }

        public DetalleSolicitudesController(
            DetalleSolicitudService detalleSolicitudService)
        {
            if (detalleSolicitudService == null)
            {
                throw new ArgumentNullException("detalleSolicitudService");
            }

            this.detalleSolicitudService = detalleSolicitudService;
        }

        // GET /api/detalle-solicitudes
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> ObtenerTodos(
            CancellationToken cancellationToken)
        {
            var detalles = await detalleSolicitudService.ObtenerTodosAsync(
                cancellationToken);

            return Ok(new ApiResponse<List<DetalleSolicitudResponse>>
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Detalles de solicitud obtenidos correctamente.",
                Data = new List<DetalleSolicitudResponse>(detalles)
            });
        }
    }
}
