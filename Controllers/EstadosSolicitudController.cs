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
    [RoutePrefix("api/estados-solicitud")]
    public class EstadosSolicitudController : ApiController
    {
        private readonly EstadoSolicitudService estadoSolicitudService;

        public EstadosSolicitudController()
            : this(DependencyConfig.CreateEstadoSolicitudService())
        {
        }

        public EstadosSolicitudController(
            EstadoSolicitudService estadoSolicitudService)
        {
            if (estadoSolicitudService == null)
            {
                throw new ArgumentNullException("estadoSolicitudService");
            }

            this.estadoSolicitudService = estadoSolicitudService;
        }

        // GET /api/estados-solicitud
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> ObtenerTodos(
            CancellationToken cancellationToken)
        {
            var estados = await estadoSolicitudService.ObtenerTodosAsync(
                cancellationToken);

            return Ok(new ApiResponse<List<EstadoSolicitudResponse>>
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Estados de solicitud obtenidos correctamente.",
                Data = new List<EstadoSolicitudResponse>(estados)
            });
        }
    }
}
