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
    [RoutePrefix("api/solicitudes")]
    public class SolicitudesController : ApiController
    {
        private readonly SolicitudService solicitudService;

        public SolicitudesController()
            : this(DependencyConfig.CreateSolicitudService())
        {
        }

        public SolicitudesController(SolicitudService solicitudService)
        {
            if (solicitudService == null)
            {
                throw new ArgumentNullException("solicitudService");
            }

            this.solicitudService = solicitudService;
        }

        // POST /api/solicitudes
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Registrar(
            [FromBody] RegistrarSolicitudRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await solicitudService.RegistrarAsync(
                    request,
                    cancellationToken);

                return Content(
                    HttpStatusCode.Created,
                    new ApiResponse<RegistrarSolicitudResponse>
                    {
                        Status = (int)HttpStatusCode.Created,
                        Message = "Solicitud registrada correctamente.",
                        Data = response
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

        // POST /api/solicitudes/flag
        [HttpPost]
        [Route("flag")]
        public async Task<IHttpActionResult> ObtenerFlag(
            [FromBody] SolicitudFlagRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentException(
                        "El cuerpo de la solicitud es obligatorio.", "request");
                }

                var solicitudes = await solicitudService.ObtenerFlagAsync(
                    request.Usuario,
                    request.Flag,
                    cancellationToken);

                return Ok(new ApiResponse<List<SolicitudFlagResponse>>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Solicitudes obtenidas correctamente por flag.",
                    Data = new List<SolicitudFlagResponse>(solicitudes)
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

        // GET /api/solicitudes/codigo/{codigoSolicitud}
        [HttpGet]
        [Route("codigo/{codigoSolicitud}")]
        public async Task<IHttpActionResult> BuscarPorCodigo(
            string codigoSolicitud,
            CancellationToken cancellationToken)
        {
            try
            {
                var solicitud = await solicitudService.BuscarPorCodigoAsync(
                    codigoSolicitud,
                    cancellationToken);

                if (solicitud == null)
                {
                    return Content(
                        HttpStatusCode.NotFound,
                        new ApiResponse<SolicitudPorCodigoResponse>
                        {
                            Status = (int)HttpStatusCode.NotFound,
                            Message = "No se encontró la solicitud indicada.",
                            Data = null
                        });
                }

                return Ok(new ApiResponse<SolicitudPorCodigoResponse>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Solicitud obtenida correctamente.",
                    Data = solicitud
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

        // POST /api/solicitudes/en-proceso
        [HttpPost]
        [Route("en-proceso")]
        public async Task<IHttpActionResult> PasarAEnProceso(
            [FromBody] SolicitudEnProcesoRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await solicitudService.PasarAEnProcesoAsync(
                    request,
                    cancellationToken);

                return Ok(new ApiResponse<SolicitudEnProcesoResponse>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "La solicitud pasó a estado En Atención correctamente.",
                    Data = response
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

        // POST /api/solicitudes/solicitar-informacion
        [HttpPost]
        [Route("solicitar-informacion")]
        public async Task<IHttpActionResult> SolicitarInformacion(
            [FromBody] SolicitarInformacionRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await solicitudService.SolicitarInformacionAsync(
                    request,
                    cancellationToken);

                return Ok(new ApiResponse<SolicitudEnProcesoResponse>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Información solicitada correctamente.",
                    Data = response
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

        // POST /api/solicitudes/enviar-informacion
        [HttpPost]
        [Route("enviar-informacion")]
        public async Task<IHttpActionResult> EnviarInformacion(
            [FromBody] EnviarInformacionRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await solicitudService.EnviarInformacionAsync(
                    request,
                    cancellationToken);

                return Ok(new ApiResponse<SolicitudEnProcesoResponse>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Información enviada correctamente.",
                    Data = response
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

        // POST /api/solicitudes/anular
        [HttpPost]
        [Route("anular")]
        public async Task<IHttpActionResult> AnularSolicitud(
            [FromBody] AnularSolicitudRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await solicitudService.AnularSolicitudAsync(
                    request,
                    cancellationToken);

                return Ok(new ApiResponse<SolicitudEnProcesoResponse>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Solicitud anulada correctamente.",
                    Data = response
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

        // POST /api/solicitudes/finalizar
        [HttpPost]
        [Route("finalizar")]
        public async Task<IHttpActionResult> FinalizarSolicitud(
            [FromBody] FinalizarSolicitudRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await solicitudService.FinalizarSolicitudAsync(
                    request,
                    cancellationToken);

                return Ok(new ApiResponse<SolicitudEnProcesoResponse>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Solicitud finalizada correctamente.",
                    Data = response
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

        // POST /api/solicitudes/reasignar
        [HttpPost]
        [Route("reasignar")]
        public async Task<IHttpActionResult> ReasignarSolicitud(
            [FromBody] ReasignarSolicitudRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await solicitudService.ReasignarSolicitudAsync(
                    request,
                    cancellationToken);

                return Ok(new ApiResponse<SolicitudEnProcesoResponse>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Solicitud reasignada correctamente.",
                    Data = response
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

        // POST /api/solicitudes/derivar
        [HttpPost]
        [Route("derivar")]
        public async Task<IHttpActionResult> DerivarSolicitud(
            [FromBody] DerivarSolicitudRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await solicitudService.DerivarSolicitudAsync(
                    request,
                    cancellationToken);

                return Ok(new ApiResponse<SolicitudEnProcesoResponse>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Solicitud derivada correctamente.",
                    Data = response
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
