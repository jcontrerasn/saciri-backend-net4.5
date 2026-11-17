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
    [RoutePrefix("api/categorias")]
    public class CategoriasController : ApiController
    {
        private readonly CategoriaService categoriaService;

        public CategoriasController()
            : this(DependencyConfig.CreateCategoriaService())
        {
        }

        public CategoriasController(CategoriaService categoriaService)
        {
            if (categoriaService == null)
            {
                throw new ArgumentNullException("categoriaService");
            }

            this.categoriaService = categoriaService;
        }

        // GET /api/categorias/tipo-registro/{idTipoRegistro}
        [HttpGet]
        [Route("tipo-registro/{idTipoRegistro:int}")]
        public async Task<IHttpActionResult> ObtenerPorTipoRegistro(
            int idTipoRegistro,
            CancellationToken cancellationToken)
        {
            try
            {
                var categorias = await categoriaService
                    .ObtenerPorTipoRegistroAsync(
                        idTipoRegistro,
                        cancellationToken);

                return Ok(new ApiResponse<List<CategoriaResponse>>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Categorías obtenidas correctamente por tipo de registro.",
                    Data = new List<CategoriaResponse>(categorias)
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
