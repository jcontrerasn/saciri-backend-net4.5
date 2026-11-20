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
    [RoutePrefix("api/catsubocu")]
    public class CatSubOcuController : ApiController
    {
        private readonly CatSubOcuService catSubOcuService;

        public CatSubOcuController()
            : this(DependencyConfig.CreateCatSubOcuService())
        {
        }

        public CatSubOcuController(CatSubOcuService catSubOcuService)
        {
            if (catSubOcuService == null)
            {
                throw new ArgumentNullException("catSubOcuService");
            }

            this.catSubOcuService = catSubOcuService;
        }

        // POST /api/catsubocu
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Crear(
            [FromBody] CrearCatSubOcuRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var registro = await catSubOcuService.CrearAsync(
                    request,
                    cancellationToken);

                return Content(
                    HttpStatusCode.Created,
                    new ApiResponse<CatSubOcuResponse>
                    {
                        Status = (int)HttpStatusCode.Created,
                        Message = "Configuración CatSubOcu creada correctamente.",
                        Data = registro
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

        // GET /api/catsubocu
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> ObtenerTodos(
            CancellationToken cancellationToken)
        {
            var registros = await catSubOcuService.ObtenerTodosAsync(
                cancellationToken);

            return Ok(new ApiResponse<List<CatSubOcuResponse>>
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Configuraciones CatSubOcu obtenidas correctamente.",
                Data = new List<CatSubOcuResponse>(registros)
            });
        }
    }
}
