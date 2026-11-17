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
    [RoutePrefix("api/ocurrencias")]
    public class OcurrenciasController : ApiController
    {
        private readonly OcurrenciaService ocurrenciaService;

        public OcurrenciasController()
            : this(DependencyConfig.CreateOcurrenciaService())
        {
        }

        public OcurrenciasController(OcurrenciaService ocurrenciaService)
        {
            if (ocurrenciaService == null)
            {
                throw new ArgumentNullException("ocurrenciaService");
            }

            this.ocurrenciaService = ocurrenciaService;
        }

        // GET /api/ocurrencias/subcategoria/{idSubcategoria}
        [HttpGet]
        [Route("subcategoria/{idSubcategoria:int}")]
        public async Task<IHttpActionResult> ObtenerPorSubcategoria(
            int idSubcategoria,
            CancellationToken cancellationToken)
        {
            try
            {
                var ocurrencias = await ocurrenciaService
                    .ObtenerPorSubcategoriaAsync(
                        idSubcategoria,
                        cancellationToken);

                return Ok(new ApiResponse<List<OcurrenciaResponse>>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Ocurrencias obtenidas correctamente por subcategoría.",
                    Data = new List<OcurrenciaResponse>(ocurrencias)
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
