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
    [RoutePrefix("api/puestos")]
    public class PuestosController : ApiController
    {
        private readonly PuestoService puestoService;

        public PuestosController()
            : this(DependencyConfig.CreatePuestoService())
        {
        }

        public PuestosController(PuestoService puestoService)
        {
            if (puestoService == null)
            {
                throw new ArgumentNullException("puestoService");
            }

            this.puestoService = puestoService;
        }

        // POST /api/puestos
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Crear(
            [FromBody] CrearPuestoRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var puesto = await puestoService.CrearAsync(
                    request,
                    cancellationToken);

                return Content(
                    HttpStatusCode.Created,
                    new ApiResponse<PuestoResponse>
                    {
                        Status = (int)HttpStatusCode.Created,
                        Message = "Puesto creado correctamente.",
                        Data = puesto
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

        // GET /api/puestos
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> ObtenerTodos(
            CancellationToken cancellationToken)
        {
            var puestos = await puestoService.ObtenerTodosAsync(
                cancellationToken);

            return Ok(new ApiResponse<List<PuestoResponse>>
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Puestos obtenidos correctamente.",
                Data = new List<PuestoResponse>(puestos)
            });
        }

        // PUT /api/puestos/{idPuesto}
        [HttpPut]
        [Route("{idPuesto:int}")]
        public async Task<IHttpActionResult> Actualizar(
            int idPuesto,
            [FromBody] ActualizarPuestoRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var puesto = await puestoService.ActualizarAsync(
                    idPuesto,
                    request,
                    cancellationToken);

                if (puesto == null)
                {
                    return Content(
                        HttpStatusCode.NotFound,
                        new ApiResponse<PuestoResponse>
                        {
                            Status = (int)HttpStatusCode.NotFound,
                            Message = "No se encontrÃ³ el puesto indicado.",
                            Data = null
                        });
                }

                return Ok(new ApiResponse<PuestoResponse>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Puesto actualizado correctamente.",
                    Data = puesto
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

        // PATCH /api/puestos/{idPuesto}/estado
        [HttpPatch]
        [Route("{idPuesto:int}/estado")]
        public async Task<IHttpActionResult> CambiarEstado(
            int idPuesto,
            CancellationToken cancellationToken)
        {
            try
            {
                var puesto = await puestoService.CambiarEstadoAsync(
                    idPuesto,
                    cancellationToken);

                if (puesto == null)
                {
                    return Content(
                        HttpStatusCode.NotFound,
                        new ApiResponse<PuestoResponse>
                        {
                            Status = (int)HttpStatusCode.NotFound,
                            Message = "No se encontrÃ³ el puesto indicado.",
                            Data = null
                        });
                }

                return Ok(new ApiResponse<PuestoResponse>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Estado del puesto actualizado correctamente.",
                    Data = puesto
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
