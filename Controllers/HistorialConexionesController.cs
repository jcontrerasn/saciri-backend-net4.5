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
    [RoutePrefix("api/historial-conexiones")]
    public class HistorialConexionesController : ApiController
    {
        private readonly HistorialConexionService historialConexionService;

        public HistorialConexionesController()
            : this(DependencyConfig.CreateHistorialConexionService())
        {
        }

        public HistorialConexionesController(
            HistorialConexionService historialConexionService)
        {
            if (historialConexionService == null)
            {
                throw new ArgumentNullException("historialConexionService");
            }

            this.historialConexionService = historialConexionService;
        }

        // POST /api/historial-conexiones
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Crear(
            [FromBody] RegistrarHistorialConexionRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var registro = await historialConexionService.CrearAsync(
                    request,
                    cancellationToken);

                return Content(
                    HttpStatusCode.Created,
                    new ApiResponse<HistorialConexionResponse>
                    {
                        Status = (int)HttpStatusCode.Created,
                        Message = "Historial de conexiÃ³n registrado correctamente.",
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

        // GET /api/historial-conexiones/usuario/{usuario}
        [HttpGet]
        [Route("usuario/{usuario}")]
        public async Task<IHttpActionResult> ObtenerPorUsuario(
            string usuario,
            CancellationToken cancellationToken)
        {
            try
            {
                var registros = await historialConexionService
                    .ObtenerPorUsuarioAsync(
                        usuario,
                        cancellationToken);

                return Ok(new ApiResponse<List<HistorialConexionResponse>>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Historial de conexiones obtenido correctamente.",
                    Data = new List<HistorialConexionResponse>(registros)
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
