using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Dtos;
using SACIRI_backend.Interfaces;

namespace SACIRI_backend.Services
{
    public class SolicitudService
    {
        private readonly ISolicitudRepository repository;
        private readonly ISolicitudPorCodigoRepository repositoryPorCodigo;

        public SolicitudService(ISolicitudRepository repository)
            : this(repository, null)
        {
        }

        public SolicitudService(
            ISolicitudRepository repository,
            ISolicitudPorCodigoRepository repositoryPorCodigo)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.repository = repository;
            this.repositoryPorCodigo = repositoryPorCodigo;
        }

        public async Task<RegistrarSolicitudResponse> RegistrarAsync(
            RegistrarSolicitudRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentException(
                    "El cuerpo de la solicitud es obligatorio.", "request");
            }

            if (string.IsNullOrWhiteSpace(request.CreadoPor))
            {
                throw new ArgumentException(
                    "El campo CreadoPor es obligatorio.", "CreadoPor");
            }

            if (request.IdTipoRegistro <= 0)
            {
                throw new ArgumentException(
                    "El identificador del tipo de registro debe ser mayor que cero.",
                    "IdTipoRegistro");
            }

            if (request.IdCategoria <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la categoría debe ser mayor que cero.",
                    "IdCategoria");
            }

            if (string.IsNullOrWhiteSpace(request.CentroCosto))
            {
                throw new ArgumentException(
                    "El campo CentroCosto es obligatorio.", "CentroCosto");
            }

            if (string.IsNullOrWhiteSpace(request.Lugar))
            {
                throw new ArgumentException(
                    "El campo Lugar es obligatorio.", "Lugar");
            }

            if (string.IsNullOrWhiteSpace(request.Descripcion))
            {
                throw new ArgumentException(
                    "El campo Descripcion es obligatorio.", "Descripcion");
            }

            var registro = await repository.RegistrarAsync(
                request,
                cancellationToken).ConfigureAwait(false);

            return registro;
        }

        public async Task<IList<SolicitudFlagResponse>> ObtenerFlagAsync(
            string usuario,
            string flag,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                throw new ArgumentException(
                    "El usuario es obligatorio.", "usuario");
            }

            var flagNormalizado = (flag ?? string.Empty).Trim().ToUpperInvariant();

            if (flagNormalizado != "CS"
                && flagNormalizado != "A"
                && flagNormalizado != "TMS")
            {
                throw new ArgumentException(
                    "El flag debe ser CS, A o TMS.", "flag");
            }

            var solicitudes = await repository.ObtenerFlagAsync(
                usuario.Trim(),
                flagNormalizado,
                cancellationToken).ConfigureAwait(false);

            return solicitudes.Select(solicitud => new SolicitudFlagResponse
            {
                CodigoSolicitud = solicitud.CodigoSolicitud,
                TipoRegistro = solicitud.TipoRegistro,
                Categoria = solicitud.Categoria,
                CreadoPor = solicitud.CreadoPor,
                SolicitadoPara = solicitud.SolicitadoPara,
                AsignadoA = solicitud.AsignadoA,
                FechaSolicitud = solicitud.FechaSolicitud,
                FechaEstimada = solicitud.FechaEstimada,
                CopiaJefatura = solicitud.CopiaJefatura,
                Estado = solicitud.Estado
            }).ToList();
        }

        public async Task<SolicitudPorCodigoResponse> BuscarPorCodigoAsync(
            string codigoSolicitud,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(codigoSolicitud))
            {
                throw new ArgumentException(
                    "El código de solicitud es obligatorio.",
                    "codigoSolicitud");
            }

            if (repositoryPorCodigo == null)
            {
                throw new InvalidOperationException(
                    "El repositorio de solicitudes por código no está configurado.");
            }

            var solicitud = await repositoryPorCodigo.BuscarAsync(
                codigoSolicitud.Trim(),
                cancellationToken).ConfigureAwait(false);

            if (solicitud == null)
            {
                return null;
            }

            return new SolicitudPorCodigoResponse
            {
                IdSolicitud = solicitud.IdSolicitud,
                CodigoSolicitud = solicitud.CodigoSolicitud,
                CreadoPor = solicitud.CreadoPor,
                SolicitadoPara = solicitud.SolicitadoPara,
                Ubicacion = solicitud.Ubicacion,
                Area = solicitud.Area,
                Telefono = solicitud.Telefono,
                ContactoArea = solicitud.ContactoArea,
                Correo = solicitud.Correo,
                TelefonoContactoArea = solicitud.TelefonoContactoArea,
                TipoRegistro = solicitud.TipoRegistro,
                Estado = solicitud.Estado,
                Categoria = solicitud.Categoria,
                Subcategoria = solicitud.Subcategoria,
                Ocurrencia = solicitud.Ocurrencia,
                CopiaJefatura = solicitud.CopiaJefatura,
                FechaSolicitud = solicitud.FechaSolicitud,
                AsignadoA = solicitud.AsignadoA,
                FechaInicioAtencion = solicitud.FechaInicioAtencion,
                FechaCaducidad = solicitud.FechaCaducidad,
                Comentarios = solicitud.Comentarios.Select(comentario =>
                    new SolicitudComentarioResponse
                    {
                        IdComentario = comentario.IdComentario,
                        Comentario = comentario.Comentario,
                        Tipo = comentario.Tipo,
                        Usuario = comentario.Usuario,
                        FechaRegistro = comentario.FechaRegistro
                    }).ToList(),
                CentroCosto = solicitud.CentroCosto,
                NombreCentroCosto = solicitud.NombreCentroCosto,
                Lugar = solicitud.Lugar
            };
        }

        public async Task<SolicitudEnProcesoResponse> PasarAEnProcesoAsync(
            SolicitudEnProcesoRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentException(
                    "El cuerpo de la solicitud es obligatorio.", "request");
            }

            if (string.IsNullOrWhiteSpace(request.CodigoSolicitud))
            {
                throw new ArgumentException(
                    "El código de solicitud es obligatorio.",
                    "CodigoSolicitud");
            }

            if (request.CodigoSolicitud.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "El código de solicitud no puede superar los 100 caracteres.",
                    "CodigoSolicitud");
            }

            if (string.IsNullOrWhiteSpace(request.Usuario))
            {
                throw new ArgumentException(
                    "El usuario es obligatorio.", "Usuario");
            }

            if (request.Usuario.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "El usuario no puede superar los 100 caracteres.",
                    "Usuario");
            }

            return await repository.PasarAEnProcesoAsync(
                request.CodigoSolicitud.Trim(),
                request.Usuario.Trim(),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<SolicitudEnProcesoResponse> SolicitarInformacionAsync(
            SolicitarInformacionRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentException(
                    "El cuerpo de la solicitud es obligatorio.", "request");
            }

            if (request.IdSolicitud <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la solicitud debe ser mayor que cero.",
                    "IdSolicitud");
            }

            if (string.IsNullOrWhiteSpace(request.Comentario))
            {
                throw new ArgumentException(
                    "El comentario es obligatorio.", "Comentario");
            }

            if (string.IsNullOrWhiteSpace(request.Usuario))
            {
                throw new ArgumentException(
                    "El usuario es obligatorio.", "Usuario");
            }

            if (request.Usuario.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "El usuario no puede superar los 100 caracteres.",
                    "Usuario");
            }

            return await repository.SolicitarInformacionAsync(
                request.IdSolicitud,
                request.Comentario.Trim(),
                request.Usuario.Trim(),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<SolicitudEnProcesoResponse> EnviarInformacionAsync(
            EnviarInformacionRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentException(
                    "El cuerpo de la solicitud es obligatorio.", "request");
            }

            if (request.IdSolicitud <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la solicitud debe ser mayor que cero.",
                    "IdSolicitud");
            }

            if (string.IsNullOrWhiteSpace(request.Comentario))
            {
                throw new ArgumentException(
                    "El comentario es obligatorio.", "Comentario");
            }

            if (string.IsNullOrWhiteSpace(request.Usuario))
            {
                throw new ArgumentException(
                    "El usuario es obligatorio.", "Usuario");
            }

            if (request.Usuario.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "El usuario no puede superar los 100 caracteres.",
                    "Usuario");
            }

            return await repository.EnviarInformacionAsync(
                request.IdSolicitud,
                request.Comentario.Trim(),
                request.Usuario.Trim(),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<SolicitudEnProcesoResponse> AnularSolicitudAsync(
            AnularSolicitudRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentException(
                    "El cuerpo de la solicitud es obligatorio.", "request");
            }

            if (request.IdSolicitud <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la solicitud debe ser mayor que cero.",
                    "IdSolicitud");
            }

            if (string.IsNullOrWhiteSpace(request.Comentario))
            {
                throw new ArgumentException(
                    "El comentario es obligatorio.", "Comentario");
            }

            if (string.IsNullOrWhiteSpace(request.Usuario))
            {
                throw new ArgumentException(
                    "El usuario es obligatorio.", "Usuario");
            }

            if (request.Usuario.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "El usuario no puede superar los 100 caracteres.",
                    "Usuario");
            }

            return await repository.AnularSolicitudAsync(
                request.IdSolicitud,
                request.Comentario.Trim(),
                request.Usuario.Trim(),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<SolicitudEnProcesoResponse> FinalizarSolicitudAsync(
            FinalizarSolicitudRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentException(
                    "El cuerpo de la solicitud es obligatorio.", "request");
            }

            if (request.IdSolicitud <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la solicitud debe ser mayor que cero.",
                    "IdSolicitud");
            }

            if (string.IsNullOrWhiteSpace(request.Comentario))
            {
                throw new ArgumentException(
                    "El comentario es obligatorio.", "Comentario");
            }

            if (string.IsNullOrWhiteSpace(request.Usuario))
            {
                throw new ArgumentException(
                    "El usuario es obligatorio.", "Usuario");
            }

            if (request.Usuario.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "El usuario no puede superar los 100 caracteres.",
                    "Usuario");
            }

            return await repository.FinalizarSolicitudAsync(
                request.IdSolicitud,
                request.Comentario.Trim(),
                request.Usuario.Trim(),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<SolicitudEnProcesoResponse> ReasignarSolicitudAsync(
            ReasignarSolicitudRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentException(
                    "El cuerpo de la solicitud es obligatorio.", "request");
            }

            if (request.IdSolicitud <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la solicitud debe ser mayor que cero.",
                    "IdSolicitud");
            }

            if (string.IsNullOrWhiteSpace(request.Comentario))
            {
                throw new ArgumentException(
                    "El comentario es obligatorio.", "Comentario");
            }

            if (string.IsNullOrWhiteSpace(request.Usuario))
            {
                throw new ArgumentException(
                    "El usuario es obligatorio.", "Usuario");
            }

            if (request.Usuario.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "El usuario no puede superar los 100 caracteres.",
                    "Usuario");
            }

            if (string.IsNullOrWhiteSpace(request.AsignadoA))
            {
                throw new ArgumentException(
                    "El usuario asignado es obligatorio.", "AsignadoA");
            }

            if (request.AsignadoA.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "El usuario asignado no puede superar los 100 caracteres.",
                    "AsignadoA");
            }

            return await repository.ReasignarSolicitudAsync(
                request.IdSolicitud,
                request.Comentario.Trim(),
                request.Usuario.Trim(),
                request.AsignadoA.Trim(),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<SolicitudEnProcesoResponse> DerivarSolicitudAsync(
            DerivarSolicitudRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentException(
                    "El cuerpo de la solicitud es obligatorio.", "request");
            }

            if (request.IdSolicitud <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la solicitud debe ser mayor que cero.",
                    "IdSolicitud");
            }

            if (string.IsNullOrWhiteSpace(request.Comentario))
            {
                throw new ArgumentException(
                    "El comentario es obligatorio.", "Comentario");
            }

            if (string.IsNullOrWhiteSpace(request.Usuario))
            {
                throw new ArgumentException(
                    "El usuario es obligatorio.", "Usuario");
            }

            if (request.Usuario.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "El usuario no puede superar los 100 caracteres.",
                    "Usuario");
            }

            if (string.IsNullOrWhiteSpace(request.AsignadoA))
            {
                throw new ArgumentException(
                    "El usuario asignado es obligatorio.", "AsignadoA");
            }

            if (request.AsignadoA.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "El usuario asignado no puede superar los 100 caracteres.",
                    "AsignadoA");
            }

            return await repository.DerivarSolicitudAsync(
                request.IdSolicitud,
                request.Comentario.Trim(),
                request.Usuario.Trim(),
                request.AsignadoA.Trim(),
                cancellationToken).ConfigureAwait(false);
        }
    }
}
