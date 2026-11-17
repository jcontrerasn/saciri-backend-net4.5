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
    [RoutePrefix("api/centros-costo")]
    public class CentrosCostoController : ApiController
    {
        private readonly CentroCostoService centroCostoService;

        public CentrosCostoController()
            : this(DependencyConfig.CreateCentroCostoService())
        {
        }

        public CentrosCostoController(CentroCostoService centroCostoService)
        {
            if (centroCostoService == null)
            {
                throw new ArgumentNullException("centroCostoService");
            }

            this.centroCostoService = centroCostoService;
        }

        // POST /api/centros-costo
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> BuscarPorFiltro(
            [FromBody] CentroCostoFiltroRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentException(
                        "El cuerpo de la solicitud es obligatorio.", "request");
                }

                var centrosCosto = await centroCostoService
                    .BuscarPorFiltroAsync(
                        request.Pfiltro_c,
                        cancellationToken);

                return Ok(new ApiResponse<List<CentroCostoResponse>>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Centros de costo obtenidos correctamente.",
                    Data = new List<CentroCostoResponse>(centrosCosto)
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
