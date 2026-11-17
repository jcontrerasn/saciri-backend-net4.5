using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Dtos;
using SACIRI_backend.Interfaces;

namespace SACIRI_backend.Services
{
    public class ComentarioSolicitudService
    {
        private readonly IComentarioSolicitudRepository repository;

        public ComentarioSolicitudService(
            IComentarioSolicitudRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.repository = repository;
        }

        public async Task<IList<ComentarioSolicitudResponse>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var comentarios = await repository.ObtenerTodosAsync(
                cancellationToken).ConfigureAwait(false);

            return comentarios.Select(comentario => new ComentarioSolicitudResponse
            {
                IdComentario = comentario.IdComentario,
                IdSolicitud = comentario.IdSolicitud,
                Comentario = comentario.Comentario,
                Tipo = comentario.Tipo,
                Usuario = comentario.Usuario,
                FechaRegistro = comentario.FechaRegistro
            }).ToList();
        }
    }
}
