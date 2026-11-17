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
    [RoutePrefix("api/tipos-registros")]
    public class TipoRegistrosController : ApiController
    {
        private readonly TipoRegistroService tipoRegistroService;

        public TipoRegistrosController()
            : this(DependencyConfig.CreateTipoRegistroService())
        {
        }

        public TipoRegistrosController(TipoRegistroService tipoRegistroService)
        {
            if (tipoRegistroService == null)
            {
                throw new ArgumentNullException("tipoRegistroService");
            }

            this.tipoRegistroService = tipoRegistroService;
        }

        // GET /api/tipos-registros
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> ObtenerTodos(
            CancellationToken cancellationToken)
        {
            var tiposRegistro = await tipoRegistroService.ObtenerTodosAsync(
                cancellationToken);

            return Ok(new ApiResponse<List<TipoRegistroResponse>>
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Tipos de registro obtenidos correctamente.",
                Data = new List<TipoRegistroResponse>(tiposRegistro)
            });
        }
    }
}
