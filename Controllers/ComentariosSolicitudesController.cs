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
    [RoutePrefix("api/comentarios-solicitudes")]
    public class ComentariosSolicitudesController : ApiController
    {
        private readonly ComentarioSolicitudService comentarioSolicitudService;

        public ComentariosSolicitudesController()
            : this(DependencyConfig.CreateComentarioSolicitudService())
        {
        }

        public ComentariosSolicitudesController(
            ComentarioSolicitudService comentarioSolicitudService)
        {
            if (comentarioSolicitudService == null)
            {
                throw new ArgumentNullException("comentarioSolicitudService");
            }

            this.comentarioSolicitudService = comentarioSolicitudService;
        }

        // GET /api/comentarios-solicitudes
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> ObtenerTodos(
            CancellationToken cancellationToken)
        {
            var comentarios = await comentarioSolicitudService.ObtenerTodosAsync(
                cancellationToken);

            return Ok(new ApiResponse<List<ComentarioSolicitudResponse>>
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Comentarios de solicitud obtenidos correctamente.",
                Data = new List<ComentarioSolicitudResponse>(comentarios)
            });
        }
    }
}
