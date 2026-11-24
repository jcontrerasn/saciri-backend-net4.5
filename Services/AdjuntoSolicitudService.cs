using System;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Dtos;
using SACIRI_backend.Entities;
using SACIRI_backend.Interfaces;

namespace SACIRI_backend.Services
{
    public class AdjuntoSolicitudService
    {
        private readonly IAdjuntoSolicitudRepository repository;

        public AdjuntoSolicitudService(IAdjuntoSolicitudRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.repository = repository;
        }

        public async Task<AdjuntoSolicitudResponse> CrearAsync(
            int idSolicitud,
            string nombreAdjunto,
            string formato,
            long tamanoAdjunto,
            string ruta,
            string adjuntadoPor,
            CancellationToken cancellationToken = default(CancellationToken),
            bool primerHistorial = false)
        {
            if (idSolicitud <= 0)
            {
                throw new ArgumentException(
                    "El idSolicitud debe ser mayor que cero.",
                    "idSolicitud");
            }

            if (string.IsNullOrWhiteSpace(nombreAdjunto)
                || nombreAdjunto.Trim().Length > 255)
            {
                throw new ArgumentException(
                    "El nombre del archivo es obligatorio y no puede superar los 255 caracteres.",
                    "nombreAdjunto");
            }

            if (string.IsNullOrWhiteSpace(formato)
                || formato.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "El formato del archivo es obligatorio y no puede superar los 100 caracteres.",
                    "formato");
            }

            if (tamanoAdjunto <= 0)
            {
                throw new ArgumentException(
                    "El archivo debe tener un tamaÃ±o mayor que cero.",
                    "tamanoAdjunto");
            }

            if (string.IsNullOrWhiteSpace(ruta)
                || ruta.Trim().Length > 500)
            {
                throw new ArgumentException(
                    "La ruta es obligatoria y no puede superar los 500 caracteres.",
                    "ruta");
            }

            if (string.IsNullOrWhiteSpace(adjuntadoPor)
                || adjuntadoPor.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "AdjuntadoPor es obligatorio y no puede superar los 100 caracteres.",
                    "adjuntadoPor");
            }

            var registro = await repository.CrearAsync(
                idSolicitud,
                nombreAdjunto.Trim(),
                formato.Trim(),
                tamanoAdjunto,
                ruta.Trim(),
                adjuntadoPor.Trim(),
                cancellationToken,
                primerHistorial).ConfigureAwait(false);

            return registro == null ? null : MapearRespuesta(registro);
        }

        private static AdjuntoSolicitudResponse MapearRespuesta(
            AdjuntoSolicitud registro)
        {
            return new AdjuntoSolicitudResponse
            {
                IdAdjunto = registro.IdAdjunto,
                IdSolicitud = registro.IdSolicitud,
                IdHistorialSolicitud = registro.IdHistorialSolicitud,
                NombreAdjunto = registro.NombreAdjunto,
                Formato = registro.Formato,
                TamanoAdjunto = registro.TamanoAdjunto,
                Ruta = registro.Ruta,
                AdjuntadoPor = registro.AdjuntadoPor,
                Fecha = registro.Fecha,
                Estado = registro.Estado
            };
        }
    }
}
