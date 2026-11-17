using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using SACIRI_backend.Dtos;
using SACIRI_backend.Services;

namespace SACIRI_backend.Controllers
{
    [RoutePrefix("api/usuarios")]
    public class UsuariosController : ApiController
    {
        private readonly UsuarioService usuarioService;

        public UsuariosController()
            : this(DependencyConfig.CreateUsuarioService())
        {
        }

        public UsuariosController(UsuarioService usuarioService)
        {
            if (usuarioService == null)
            {
                throw new ArgumentNullException("usuarioService");
            }

            this.usuarioService = usuarioService;
        }

        // GET /api/usuarios/cuenta/{cuenta}
        [HttpGet]
        [Route("cuenta/{cuenta}")]
        public async Task<IHttpActionResult> BuscarPorCuenta(
            string cuenta,
            CancellationToken cancellationToken)
        {
            try
            {
                var usuario = await usuarioService.BuscarPorCuentaAsync(
                    cuenta, cancellationToken);

                if (usuario == null)
                {
                    return Content(
                        HttpStatusCode.NotFound,
                        new ApiResponse<UsuarioCuentaResponse>
                        {
                            Status = (int)HttpStatusCode.NotFound,
                            Message = "No se encontró un usuario para la cuenta indicada.",
                            Data = null
                        });
                }

                return Ok(new ApiResponse<UsuarioCuentaResponse>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Usuario obtenido correctamente.",
                    Data = usuario
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
