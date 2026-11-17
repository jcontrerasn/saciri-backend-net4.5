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
    [RoutePrefix("api/usuarios-roles")]
    public class UsuariosRolesController : ApiController
    {
        private readonly UsuarioRolService usuarioRolService;

        public UsuariosRolesController()
            : this(DependencyConfig.CreateUsuarioRolService())
        {
        }

        public UsuariosRolesController(UsuarioRolService usuarioRolService)
        {
            if (usuarioRolService == null)
            {
                throw new ArgumentNullException("usuarioRolService");
            }

            this.usuarioRolService = usuarioRolService;
        }

        // POST /api/usuarios-roles
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Crear(
            [FromBody] CrearUsuarioRolRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var usuarioRol = await usuarioRolService.CrearAsync(
                    request,
                    cancellationToken);

                return Content(
                    HttpStatusCode.Created,
                    new ApiResponse<UsuarioRolResponse>
                    {
                        Status = (int)HttpStatusCode.Created,
                        Message = "Usuario asignado al rol correctamente.",
                        Data = usuarioRol
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

        // PUT /api/usuarios-roles/{idUsuarioRol}
        [HttpPut]
        [Route("{idUsuarioRol:int}")]
        public async Task<IHttpActionResult> Actualizar(
            int idUsuarioRol,
            [FromBody] ActualizarUsuarioRolRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var usuarioRol = await usuarioRolService.ActualizarAsync(
                    idUsuarioRol,
                    request,
                    cancellationToken);

                if (usuarioRol == null)
                {
                    return Content(
                        HttpStatusCode.NotFound,
                        new ApiResponse<UsuarioRolResponse>
                        {
                            Status = (int)HttpStatusCode.NotFound,
                            Message = "No se encontró la asignación de usuario indicada.",
                            Data = null
                        });
                }

                return Ok(new ApiResponse<UsuarioRolResponse>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Rol y jefe actualizados correctamente.",
                    Data = usuarioRol
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

        // PATCH /api/usuarios-roles/{idUsuarioRol}/estado
        [HttpPatch]
        [Route("{idUsuarioRol:int}/estado")]
        public async Task<IHttpActionResult> CambiarEstado(
            int idUsuarioRol,
            CancellationToken cancellationToken)
        {
            try
            {
                var usuarioRol = await usuarioRolService.CambiarEstadoAsync(
                    idUsuarioRol,
                    cancellationToken);

                if (usuarioRol == null)
                {
                    return Content(
                        HttpStatusCode.NotFound,
                        new ApiResponse<UsuarioRolResponse>
                        {
                            Status = (int)HttpStatusCode.NotFound,
                            Message = "No se encontró la asignación de usuario indicada.",
                            Data = null
                        });
                }

                return Ok(new ApiResponse<UsuarioRolResponse>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Estado actualizado correctamente.",
                    Data = usuarioRol
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

        // GET /api/usuarios-roles
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> ObtenerTodos(
            CancellationToken cancellationToken)
        {
            var usuariosRoles = await usuarioRolService.ObtenerTodosAsync(
                cancellationToken);

            return Ok(new ApiResponse<List<UsuarioRolResponse>>
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Usuarios con roles obtenidos correctamente.",
                Data = new List<UsuarioRolResponse>(usuariosRoles)
            });
        }
    }
}
