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
    [RoutePrefix("api/roles")]
    public class RolesController : ApiController
    {
        private readonly RolService rolService;

        public RolesController()
            : this(DependencyConfig.CreateRolService())
        {
        }

        public RolesController(RolService rolService)
        {
            if (rolService == null)
            {
                throw new ArgumentNullException("rolService");
            }

            this.rolService = rolService;
        }

        // GET /api/roles
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> ObtenerTodos(
            CancellationToken cancellationToken)
        {
            var roles = await rolService.ObtenerTodosAsync(
                cancellationToken);

            return Ok(new ApiResponse<List<RolResponse>>
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Roles obtenidos correctamente.",
                Data = new List<RolResponse>(roles)
            });
        }
    }
}
