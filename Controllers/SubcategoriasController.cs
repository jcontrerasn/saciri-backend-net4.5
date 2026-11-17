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

        // POST /api/subcategorias
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Crear(
            [FromBody] CrearSubcategoriaRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var subcategoria = await subcategoriaService.CrearAsync(
                    request,
                    cancellationToken);

                return Content(
                    HttpStatusCode.Created,
                    new ApiResponse<SubcategoriaResponse>
                    {
                        Status = (int)HttpStatusCode.Created,
                        Message = "Subcategoría creada correctamente.",
                        Data = subcategoria
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

        // PUT /api/subcategorias/{idSubcategoria}
        [HttpPut]
        [Route("{idSubcategoria:int}")]
        public async Task<IHttpActionResult> Actualizar(
            int idSubcategoria,
            [FromBody] ActualizarSubcategoriaRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var subcategoria = await subcategoriaService.ActualizarAsync(
                    idSubcategoria,
                    request,
                    cancellationToken);

                if (subcategoria == null)
                {
                    return Content(
                        HttpStatusCode.NotFound,
                        new ApiResponse<SubcategoriaResponse>
                        {
                            Status = (int)HttpStatusCode.NotFound,
                            Message = "No se encontró la subcategoría indicada.",
                            Data = null
                        });
                }

                return Ok(new ApiResponse<SubcategoriaResponse>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Subcategoría actualizada correctamente.",
                    Data = subcategoria
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

        // PATCH /api/subcategorias/{idSubcategoria}/estado
        [HttpPatch]
        [Route("{idSubcategoria:int}/estado")]
        public async Task<IHttpActionResult> CambiarEstado(
            int idSubcategoria,
            CancellationToken cancellationToken)
        {
            var subcategoria = await subcategoriaService.CambiarEstadoAsync(
                idSubcategoria,
                cancellationToken);

            if (subcategoria == null)
            {
                return Content(
                    HttpStatusCode.NotFound,
                    new ApiResponse<SubcategoriaResponse>
                    {
                        Status = (int)HttpStatusCode.NotFound,
                        Message = "No se encontró la subcategoría indicada.",
                        Data = null
                    });
            }

            return Ok(new ApiResponse<SubcategoriaResponse>
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Estado de la subcategoría actualizado correctamente.",
                Data = subcategoria
            });
        }

        // GET /api/subcategorias
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> ObtenerTodas(
            CancellationToken cancellationToken)
        {
            var subcategorias = await subcategoriaService.ObtenerTodasAsync(
                cancellationToken);

            return Ok(new ApiResponse<List<SubcategoriaResponse>>
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Subcategorías obtenidas correctamente.",
                Data = new List<SubcategoriaResponse>(subcategorias)
            });
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
