using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http.Formatting;
using SACIRI_backend.Dtos;
using SACIRI_backend.Services;

namespace SACIRI_backend.Controllers
{
    [RoutePrefix("api/solicitudes")]
    public class AdjuntosSolicitudesController : ApiController
    {
        private readonly AdjuntoSolicitudService adjuntoSolicitudService;

        public AdjuntosSolicitudesController()
            : this(DependencyConfig.CreateAdjuntoSolicitudService())
        {
        }

        public AdjuntosSolicitudesController(
            AdjuntoSolicitudService adjuntoSolicitudService)
        {
            if (adjuntoSolicitudService == null)
            {
                throw new ArgumentNullException("adjuntoSolicitudService");
            }

            this.adjuntoSolicitudService = adjuntoSolicitudService;
        }

        // POST /api/solicitudes/{idSolicitud}/adjuntos
        [HttpPost]
        [Route("{idSolicitud:int}/adjuntos")]
        public async Task<IHttpActionResult> Crear(
            int idSolicitud,
            CancellationToken cancellationToken)
        {
            if (!Request.Content.IsMimeMultipartContent())
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
            string rutaFisica = null;

            try
            {
                await Request.Content.ReadAsMultipartAsync(
                    provider,
                    cancellationToken).ConfigureAwait(false);

                var archivo = provider.FileData.FirstOrDefault();
                if (archivo == null)
                {
                    return Content(
                        HttpStatusCode.BadRequest,
                        new ApiResponse<object>
                        {
                            Status = (int)HttpStatusCode.BadRequest,
                            Message = "Debe enviar un archivo en el campo 'archivo'.",
                            Data = null
                        });
                }

                var contentDisposition = archivo.Headers.ContentDisposition;
                var nombreOriginal = contentDisposition == null
                    ? null
                    : contentDisposition.FileName;
                var nombreAdjunto = string.IsNullOrWhiteSpace(nombreOriginal)
                    ? null
                    : Path.GetFileName(nombreOriginal.Trim('"'));
                if (string.IsNullOrWhiteSpace(nombreAdjunto))
                {
                    return Content(
                        HttpStatusCode.BadRequest,
                        new ApiResponse<object>
                        {
                            Status = (int)HttpStatusCode.BadRequest,
                            Message = "El archivo debe tener un nombre válido.",
                            Data = null
                        });
                }

                var adjuntadoPor = provider.FormData["adjuntadoPor"]
                    ?? provider.FormData["AdjuntadoPor"];
                var extension = Path.GetExtension(nombreAdjunto);
                var formato = string.IsNullOrWhiteSpace(extension)
                    ? archivo.Headers.ContentType == null
                        ? null
                        : archivo.Headers.ContentType.MediaType
                    : extension.TrimStart('.');

                var nombreSeguro = SanitizarNombre(nombreAdjunto);
                var nombreGuardado = Guid.NewGuid().ToString("N")
                    + "_" + nombreSeguro;
                var directorioSolicitud = Path.Combine(
                    rootDirectory,
                    "uploads",
                    "solicitudes",
                    idSolicitud.ToString());
                Directory.CreateDirectory(directorioSolicitud);

                rutaFisica = Path.Combine(directorioSolicitud, nombreGuardado);
                File.Move(archivo.LocalFileName, rutaFisica);

                var ruta = "uploads/solicitudes/" + idSolicitud + "/"
                    + nombreGuardado;
                var tamanoAdjunto = new FileInfo(rutaFisica).Length;

                var adjunto = await adjuntoSolicitudService.CrearAsync(
                    idSolicitud,
                    nombreAdjunto,
                    formato,
                    tamanoAdjunto,
                    ruta,
                    adjuntadoPor,
                    cancellationToken).ConfigureAwait(false);

                if (adjunto == null)
                {
                    EliminarArchivo(rutaFisica);
                    return Content(
                        HttpStatusCode.NotFound,
                        new ApiResponse<AdjuntoSolicitudResponse>
                        {
                            Status = (int)HttpStatusCode.NotFound,
                            Message = "No se encontró un historial para la solicitud indicada.",
                            Data = null
                        });
                }

                return Content(
                    HttpStatusCode.Created,
                    new ApiResponse<AdjuntoSolicitudResponse>
                    {
                        Status = (int)HttpStatusCode.Created,
                        Message = "Adjunto registrado correctamente.",
                        Data = adjunto
                    });
            }
            catch (ArgumentException exception)
            {
                EliminarArchivo(rutaFisica);
                return Content(
                    HttpStatusCode.BadRequest,
                    new ApiResponse<object>
                    {
                        Status = (int)HttpStatusCode.BadRequest,
                        Message = exception.Message,
                        Data = null
                    });
            }
            catch
            {
                EliminarArchivo(rutaFisica);
                throw;
            }
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
    }
}
