using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Dtos;
using SACIRI_backend.Interfaces;

namespace SACIRI_backend.Services
{
    public class HistorialSolicitudService
    {
        private readonly IHistorialSolicitudRepository repository;

        public HistorialSolicitudService(
            IHistorialSolicitudRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.repository = repository;
        }

        public async Task<IList<HistorialSolicitudResponse>>
            ObtenerPorSolicitudAsync(
                int idSolicitud,
                CancellationToken cancellationToken = default(CancellationToken))
        {
            if (idSolicitud <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la solicitud debe ser mayor que cero.",
                    "idSolicitud");
            }

            var historial = await repository.ObtenerPorSolicitudAsync(
                idSolicitud,
                cancellationToken).ConfigureAwait(false);

            return historial.Select(registro => new HistorialSolicitudResponse
            {
                IdHistorialSolicitud = registro.IdHistorialSolicitud,
                IdSolicitud = registro.IdSolicitud,
                SolicitadoPara = registro.SolicitadoPara,
                CreadoPor = registro.CreadoPor,
                CodigoSolicitud = registro.CodigoSolicitud,
                EstadoSolicitud = registro.EstadoSolicitud,
                FechaSolicitud = registro.FechaSolicitud,
                TipoRegistro = registro.TipoRegistro,
                Categoria = registro.Categoria,
                Subcategoria = registro.Subcategoria,
                Ocurrencia = registro.Ocurrencia,
                Prioridad = registro.Prioridad,
                AsignadoA = registro.AsignadoA,
                FechaInicio = registro.FechaInicio,
                FechaCaducidad = registro.FechaCaducidad,
                FechaFinAtencion = registro.FechaFinAtencion,
                DuracionAtencion = registro.DuracionAtencion,
                CopiaJefatura = registro.CopiaJefatura,
                JefeAprobador = registro.JefeAprobador,
                EstadoAprobacion = registro.EstadoAprobacion,
                UsuarioAprobacion = registro.UsuarioAprobacion,
                FechaAprobacion = registro.FechaAprobacion,
                Nivel = registro.Nivel,
                ProblemaMasivo = registro.ProblemaMasivo,
                CCSolicitadoPara = registro.CCSolicitadoPara,
                Accion = registro.Accion,
                UsuarioModificacion = registro.UsuarioModificacion,
                Observacion = registro.Observacion,
                EstadoAnterior = registro.EstadoAnterior,
                EstadoNuevo = registro.EstadoNuevo,
                FechaRegistro = registro.FechaRegistro
            }).ToList();
        }
    }
}
