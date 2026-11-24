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
    [RoutePrefix("api/conexiones-usuarios")]
    public class ConexionesUsuariosController : ApiController
    {
        private readonly ConexionUsuarioService conexionUsuarioService;

        public ConexionesUsuariosController()
            : this(DependencyConfig.CreateConexionUsuarioService())
        {
        }

        public ConexionesUsuariosController(
            ConexionUsuarioService conexionUsuarioService)
        {
            if (conexionUsuarioService == null)
            {
                throw new ArgumentNullException("conexionUsuarioService");
            }

            this.conexionUsuarioService = conexionUsuarioService;
        }

        // GET /api/conexiones-usuarios
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> ObtenerTodos(
            CancellationToken cancellationToken)
        {
            var conexiones = await conexionUsuarioService.ObtenerTodosAsync(
                cancellationToken);

            return Ok(new ApiResponse<List<ConexionUsuarioResponse>>
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Conexiones de usuarios obtenidas correctamente.",
                Data = new List<ConexionUsuarioResponse>(conexiones)
            });
        }

        // POST /api/conexiones-usuarios
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Crear(
            [FromBody] RegistrarConexionUsuarioRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var conexion = await conexionUsuarioService.CrearAsync(
                    request,
                    cancellationToken);

                if (conexion == null)
                {
                    return Content(
                        HttpStatusCode.NotFound,
                        new ApiResponse<ConexionUsuarioResponse>
                        {
                            Status = (int)HttpStatusCode.NotFound,
                            Message = "No existe una conexiÃ³n registrada para el usuario indicado.",
                            Data = null
                        });
                }

                return Content(
                    HttpStatusCode.Created,
                    new ApiResponse<ConexionUsuarioResponse>
                    {
                        Status = (int)HttpStatusCode.Created,
                        Message = "ConexiÃ³n de usuario registrada correctamente.",
                        Data = conexion
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

        // GET /api/conexiones-usuarios/estado/{usuario}
        [HttpGet]
        [Route("estado/{usuario}")]
        public async Task<IHttpActionResult> ObtenerEstado(
            string usuario,
            CancellationToken cancellationToken)
        {
            try
            {
                var estado = await conexionUsuarioService.ObtenerEstadoAsync(
                    usuario,
                    cancellationToken);

                if (estado == null)
                {
                    return Content(
                        HttpStatusCode.NotFound,
                        new ApiResponse<EstadoConexionUsuarioResponse>
                        {
                            Status = (int)HttpStatusCode.NotFound,
                            Message = "No existe una conexiÃ³n registrada para el usuario indicado.",
                            Data = null
                        });
                }

                return Ok(new ApiResponse<EstadoConexionUsuarioResponse>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Estado de conexiÃ³n obtenido correctamente.",
                    Data = estado
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
