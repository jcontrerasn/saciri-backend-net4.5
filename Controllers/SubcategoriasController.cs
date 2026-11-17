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
    [RoutePrefix("api/subcategorias")]
    public class SubcategoriasController : ApiController
    {
        private readonly SubcategoriaService subcategoriaService;

        public SubcategoriasController()
            : this(DependencyConfig.CreateSubcategoriaService())
        {
        }

        public SubcategoriasController(SubcategoriaService subcategoriaService)
        {
            if (subcategoriaService == null)
            {
                throw new ArgumentNullException("subcategoriaService");
            }

            this.subcategoriaService = subcategoriaService;
        }

        // GET /api/subcategorias/categoria/{idCategoria}
        [HttpGet]
        [Route("categoria/{idCategoria:int}")]
        public async Task<IHttpActionResult> ObtenerPorCategoria(
            int idCategoria,
            CancellationToken cancellationToken)
        {
            try
            {
                var subcategorias = await subcategoriaService
                    .ObtenerPorCategoriaAsync(
                        idCategoria,
                        cancellationToken);

                return Ok(new ApiResponse<List<SubcategoriaResponse>>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Subcategorías obtenidas correctamente por categoría.",
                    Data = new List<SubcategoriaResponse>(subcategorias)
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
