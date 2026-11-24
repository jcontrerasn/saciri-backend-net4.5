using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Formatting;
using System.Web.Http;
using SACIRI_backend.Dtos;
using SACIRI_backend.Services;

namespace SACIRI_backend.Controllers
{
    [RoutePrefix("api/solicitudes")]
    public class SolicitudesController : ApiController
    {
        private readonly SolicitudService solicitudService;
        private readonly AdjuntoSolicitudService adjuntoSolicitudService;

        public SolicitudesController()
            : this(
                DependencyConfig.CreateSolicitudService(),
                DependencyConfig.CreateAdjuntoSolicitudService())
        {
        }

        public SolicitudesController(
            SolicitudService solicitudService,
            AdjuntoSolicitudService adjuntoSolicitudService)
        {
            if (solicitudService == null)
            {
                throw new ArgumentNullException("solicitudService");
            }

            this.solicitudService = solicitudService;
            if (adjuntoSolicitudService == null)
            {
                throw new ArgumentNullException(
                    "adjuntoSolicitudService");
            }

            this.adjuntoSolicitudService = adjuntoSolicitudService;
        }

        // POST /api/solicitudes
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Registrar(
            CancellationToken cancellationToken)
        {
            if (Request.Content == null
                || !Request.Content.IsMimeMultipartContent())
            {
                return Content(
                    HttpStatusCode.UnsupportedMediaType,
                    new ApiResponse<object>
                    {
                        Status = (int)HttpStatusCode.UnsupportedMediaType,
                        Message = "La solicitud debe ser multipart/form-data.",
                        Data = null
                    });
            }

            var rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var tempDirectory = Path.Combine(
                rootDirectory,
                "App_Data",
                "uploads-temp");
            Directory.CreateDirectory(tempDirectory);

            var provider = new MultipartFormDataStreamProvider(tempDirectory);
            var archivosTemporales = new List<string>();
            var rutasFisicas = new List<string>();

            try
            {
                await Request.Content.ReadAsMultipartAsync(
                    provider,
                    cancellationToken).ConfigureAwait(false);

                var archivos = provider.FileData.ToList();
                if (archivos.Count == 0)
                {
                    throw new ArgumentException(
                        "Debe enviar al menos un archivo en el campo 'archivo'.");
                }

                archivosTemporales = archivos.Select(
                    item => item.LocalFileName).ToList();
                var archivosPreparados = new List<ArchivoAdjuntoTemporal>();

                foreach (var archivo in archivos)
                {
                var contentDisposition = archivo.Headers.ContentDisposition;
                var nombreOriginal = contentDisposition == null
                    ? null
                    : contentDisposition.FileName;
                var nombreAdjunto = string.IsNullOrWhiteSpace(nombreOriginal)
                    ? null
                    : Path.GetFileName(nombreOriginal.Trim('"'));

                if (string.IsNullOrWhiteSpace(nombreAdjunto))
                {
                    throw new ArgumentException(
                        "El archivo debe tener un nombre válido.");
                }

                var extension = Path.GetExtension(nombreAdjunto);
                var formato = string.IsNullOrWhiteSpace(extension)
                    ? archivo.Headers.ContentType == null
                        ? null
                        : archivo.Headers.ContentType.MediaType
                    : extension.TrimStart('.');

                var tamanoAdjunto = new FileInfo(
                    archivo.LocalFileName).Length;
                archivosPreparados.Add(new ArchivoAdjuntoTemporal
                {
                    Archivo = archivo,
                    NombreAdjunto = nombreAdjunto,
                    Formato = formato,
                    TamanoAdjunto = tamanoAdjunto
                });
                }

                var request = CrearSolicitudRequest(provider);
                var adjuntadoPor = request.CreadoPor;

                foreach (var archivoPreparado in archivosPreparados)
                {
                    ValidarDatosAdjunto(
                        archivoPreparado.NombreAdjunto,
                        archivoPreparado.Formato,
                        archivoPreparado.TamanoAdjunto,
                        adjuntadoPor);
                }

                var response = await solicitudService.RegistrarAsync(
                    request,
                    cancellationToken).ConfigureAwait(false);

                if (response == null || response.IdSolicitud <= 0)
                {
                    throw new InvalidOperationException(
                        "No se pudo obtener el IdSolicitud registrado.");
                }

                var directorioSolicitud = Path.Combine(
                    rootDirectory,
                    "uploads",
                    "solicitudes",
                    response.IdSolicitud.ToString());
                Directory.CreateDirectory(directorioSolicitud);

                foreach (var archivoPreparado in archivosPreparados)
                {
                    var nombreSeguro = SanitizarNombre(
                        archivoPreparado.NombreAdjunto);
                    var nombreGuardado = Guid.NewGuid().ToString("N")
                        + "_" + nombreSeguro;
                    var rutaFisica = Path.Combine(
                        directorioSolicitud,
                        nombreGuardado);

                    File.Move(
                        archivoPreparado.Archivo.LocalFileName,
                        rutaFisica);
                    rutasFisicas.Add(rutaFisica);

                    var ruta = "uploads/solicitudes/"
                        + response.IdSolicitud + "/" + nombreGuardado;
                    var adjunto = await adjuntoSolicitudService.CrearAsync(
                        response.IdSolicitud,
                        archivoPreparado.NombreAdjunto,
                        archivoPreparado.Formato,
                        archivoPreparado.TamanoAdjunto,
                        ruta,
                        adjuntadoPor,
                        cancellationToken,
                        true).ConfigureAwait(false);

                    if (adjunto == null)
                    {
                    throw new InvalidOperationException(
                        "No se encontró el primer historial de la solicitud.");
                    }
                }

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
                EliminarArchivos(archivosTemporales);
                EliminarArchivos(rutasFisicas);
                return Content(
                    HttpStatusCode.BadRequest,
                    new ApiResponse<object>
                    {
                        Status = (int)HttpStatusCode.BadRequest,
                        Message = exception.Message,
                        Data = null
                    });
            }
            catch (InvalidOperationException exception)
            {
                EliminarArchivos(archivosTemporales);
                EliminarArchivos(rutasFisicas);
                return Content(
                    HttpStatusCode.InternalServerError,
                    new ApiResponse<object>
                    {
                        Status = (int)HttpStatusCode.InternalServerError,
                        Message = exception.Message,
                        Data = null
                    });
            }
            catch
            {
                EliminarArchivos(archivosTemporales);
                EliminarArchivos(rutasFisicas);
                throw;
            }
        }

        private static RegistrarSolicitudRequest CrearSolicitudRequest(
            MultipartFormDataStreamProvider provider)
        {
            return new RegistrarSolicitudRequest
            {
                SolicitadoPara = ObtenerCampo(provider, "solicitadoPara"),
                CreadoPor = ObtenerCampo(provider, "creadoPor"),
                IdTipoRegistro = ObtenerEntero(provider, "idTipoRegistro"),
                IdCategoria = ObtenerEntero(provider, "idCategoria"),
                IdSubcategoria = ObtenerEnteroNullable(
                    provider, "idSubcategoria"),
                IdOcurrencia = ObtenerEnteroNullable(
                    provider, "idOcurrencia"),
                CentroCosto = ObtenerCampo(provider, "centroCosto"),
                Lugar = ObtenerCampo(provider, "lugar"),
                Descripcion = ObtenerCampo(provider, "descripcion"),
                Recomendacion = ObtenerCampo(provider, "recomendacion")
            };
        }

        private sealed class ArchivoAdjuntoTemporal
        {
            public MultipartFileData Archivo { get; set; }
            public string NombreAdjunto { get; set; }
            public string Formato { get; set; }
            public long TamanoAdjunto { get; set; }
        }

        private static string ObtenerCampo(
            MultipartFormDataStreamProvider provider,
            string nombre)
        {
            var clave = provider.FormData.AllKeys.FirstOrDefault(
                key => string.Equals(
                    key,
                    nombre,
                    StringComparison.OrdinalIgnoreCase));

            return clave == null ? null : provider.FormData[clave];
        }

        private static int ObtenerEntero(
            MultipartFormDataStreamProvider provider,
            string nombre)
        {
            int valor;
            return int.TryParse(ObtenerCampo(provider, nombre), out valor)
                ? valor
                : 0;
        }

        private static int? ObtenerEnteroNullable(
            MultipartFormDataStreamProvider provider,
            string nombre)
        {
            int valor;
            return int.TryParse(ObtenerCampo(provider, nombre), out valor)
                ? (int?)valor
                : null;
        }

        private static string SanitizarNombre(string nombre)
        {
            var caracteresInvalidos = Path.GetInvalidFileNameChars();
            return new string(nombre.Select(caracter =>
                caracteresInvalidos.Contains(caracter)
                    ? '_'
                    : caracter).ToArray());
        }

        private static void EliminarArchivo(string ruta)
        {
            if (!string.IsNullOrWhiteSpace(ruta) && File.Exists(ruta))
            {
                File.Delete(ruta);
            }
        }

        private static void EliminarArchivos(IEnumerable<string> rutas)
        {
            foreach (var ruta in rutas)
            {
                EliminarArchivo(ruta);
            }
        }

        private static void ValidarDatosAdjunto(
            string nombreAdjunto,
            string formato,
            long tamanoAdjunto,
            string adjuntadoPor)
        {
            if (nombreAdjunto.Length > 255)
            {
                throw new ArgumentException(
                    "El nombre del archivo no puede superar los 255 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(formato)
                || formato.Length > 100)
            {
                throw new ArgumentException(
                    "El formato del archivo es obligatorio y no puede superar los 100 caracteres.");
            }

            if (tamanoAdjunto <= 0)
            {
                throw new ArgumentException(
                    "El archivo debe tener un tamaño mayor que cero.");
            }

            if (string.IsNullOrWhiteSpace(adjuntadoPor)
                || adjuntadoPor.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "AdjuntadoPor es obligatorio y no puede superar los 100 caracteres.");
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
