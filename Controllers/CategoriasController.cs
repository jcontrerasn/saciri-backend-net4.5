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

        // POST /api/categorias
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Crear(
            [FromBody] CrearCategoriaRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var categoria = await categoriaService.CrearAsync(
                    request,
                    cancellationToken);

                return Content(
                    HttpStatusCode.Created,
                    new ApiResponse<CategoriaResponse>
                    {
                        Status = (int)HttpStatusCode.Created,
                        Message = "Categoría creada correctamente.",
                        Data = categoria
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

        // GET /api/categorias
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> ObtenerTodas(
            CancellationToken cancellationToken)
        {
            var categorias = await categoriaService.ObtenerTodasAsync(
                cancellationToken);

            return Ok(new ApiResponse<List<CategoriaResponse>>
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Categorías obtenidas correctamente.",
                Data = new List<CategoriaResponse>(categorias)
            });
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

        // PUT /api/categorias/{idCategoria}
        [HttpPut]
        [Route("{idCategoria:int}")]
        public async Task<IHttpActionResult> Actualizar(
            int idCategoria,
            [FromBody] ActualizarCategoriaRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var categoria = await categoriaService.ActualizarAsync(
                    idCategoria,
                    request,
                    cancellationToken);

                if (categoria == null)
                {
                    return Content(
                        HttpStatusCode.NotFound,
                        new ApiResponse<CategoriaResponse>
                        {
                            Status = (int)HttpStatusCode.NotFound,
                            Message = "No se encontró la categoría indicada.",
                            Data = null
                        });
                }

                return Ok(new ApiResponse<CategoriaResponse>
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = "Categoría actualizada correctamente.",
                    Data = categoria
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

        // PATCH /api/categorias/{idCategoria}/estado
        [HttpPatch]
        [Route("{idCategoria:int}/estado")]
        public async Task<IHttpActionResult> CambiarEstado(
            int idCategoria,
            CancellationToken cancellationToken)
        {
            var categoria = await categoriaService.CambiarEstadoAsync(
                idCategoria,
                cancellationToken);

            if (categoria == null)
            {
                return Content(
                    HttpStatusCode.NotFound,
                    new ApiResponse<CategoriaResponse>
                    {
                        Status = (int)HttpStatusCode.NotFound,
                        Message = "No se encontró la categoría indicada.",
                        Data = null
                    });
            }

            return Ok(new ApiResponse<CategoriaResponse>
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Estado de la categoría actualizado correctamente.",
                Data = categoria
            });
        }
    }
}
