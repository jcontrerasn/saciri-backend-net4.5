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

        // POST /api/ocurrencias
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Crear(
            [FromBody] CrearOcurrenciaRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var ocurrencia = await ocurrenciaService.CrearAsync(
                    request,
                    cancellationToken);

                return Content(
                    HttpStatusCode.Created,
                    new ApiResponse<OcurrenciaResponse>
                    {
                        Status = (int)HttpStatusCode.Created,
                        Message = "Ocurrencia creada correctamente.",
                        Data = ocurrencia
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

        // PUT /api/ocurrencias/{idOcurrencia}
        [HttpPut]
        [Route("{idOcurrencia:int}")]
        public async Task<IHttpActionResult> Actualizar(
            int idOcurrencia,
            [FromBody] ActualizarOcurrenciaRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var ocurrencia = await ocurrenciaService.ActualizarAsync(
                    idOcurrencia,
                    request,
                    cancellationToken);

                if (ocurrencia == null)
                {
                    return Content(
                        HttpStatusCode.NotFound,
                        new ApiResponse<OcurrenciaResponse>
                        {
                            Status = (int)HttpStatusCode.NotFound,
                            Message = "No se encontró la ocurrencia indicada.",
                            Data = null
                        });
                }

                return Ok(new ApiResponse<OcurrenciaResponse>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Ocurrencia actualizada correctamente.",
                    Data = ocurrencia
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

        // PATCH /api/ocurrencias/{idOcurrencia}/estado
        [HttpPatch]
        [Route("{idOcurrencia:int}/estado")]
        public async Task<IHttpActionResult> CambiarEstado(
            int idOcurrencia,
            CancellationToken cancellationToken)
        {
            var ocurrencia = await ocurrenciaService.CambiarEstadoAsync(
                idOcurrencia,
                cancellationToken);

            if (ocurrencia == null)
            {
                return Content(
                    HttpStatusCode.NotFound,
                    new ApiResponse<OcurrenciaResponse>
                    {
                        Status = (int)HttpStatusCode.NotFound,
                        Message = "No se encontró la ocurrencia indicada.",
                        Data = null
                    });
            }

            return Ok(new ApiResponse<OcurrenciaResponse>
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Estado de la ocurrencia actualizado correctamente.",
                Data = ocurrencia
            });
        }

        // GET /api/ocurrencias
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> ObtenerTodas(
            CancellationToken cancellationToken)
        {
            var ocurrencias = await ocurrenciaService.ObtenerTodasAsync(
                cancellationToken);

            return Ok(new ApiResponse<List<OcurrenciaResponse>>
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Ocurrencias obtenidas correctamente.",
                Data = new List<OcurrenciaResponse>(ocurrencias)
            });
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
