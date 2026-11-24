using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Dtos;
using SACIRI_backend.Interfaces;

namespace SACIRI_backend.Services
{
    public class CatSubOcuService
    {
        private readonly ICatSubOcuRepository repository;

        public CatSubOcuService(ICatSubOcuRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.repository = repository;
        }

        public async Task<CatSubOcuResponse> CrearAsync(
            CrearCatSubOcuRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentException(
                    "El cuerpo de la solicitud es obligatorio.", "request");
            }

            if (request.IdTipoRegistro <= 0
                || request.IdCategoria <= 0
                || request.IdSubcategoria <= 0
                || request.IdOcurrencia <= 0)
            {
                throw new ArgumentException(
                    "Los identificadores de tipo, categoría, subcategoría y ocurrencia deben ser mayores que cero.",
                    "request");
            }

            if (!string.IsNullOrWhiteSpace(request.NivelAprobacion)
                && request.NivelAprobacion.Trim().Length > 5)
            {
                throw new ArgumentException(
                    "NivelAprobacion no puede superar los 5 caracteres.",
                    "NivelAprobacion");
            }

            if (!string.IsNullOrWhiteSpace(request.RolAprobacion)
                && request.RolAprobacion.Trim().Length > 10)
            {
                throw new ArgumentException(
                    "RolAprobacion no puede superar los 10 caracteres.",
                    "RolAprobacion");
            }

            if (!string.IsNullOrWhiteSpace(request.Copia)
                && request.Copia.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "Copia no puede superar los 100 caracteres.",
                    "Copia");
            }

            var registro = await repository.CrearAsync(new Entities.CatSubOcu
            {
                IdTipoRegistro = request.IdTipoRegistro,
                IdCategoria = request.IdCategoria,
                IdSubcategoria = request.IdSubcategoria,
                IdOcurrencia = request.IdOcurrencia,
                RequiereAprobacion = request.RequiereAprobacion,
                NivelAprobacion = NormalizarTexto(request.NivelAprobacion),
                RolAprobacion = NormalizarTexto(request.RolAprobacion),
                TiempoAtencion = request.TiempoAtencion,
                Estado = request.Estado,
                Atencion = request.Atencion,
                Nivel1 = request.Nivel1,
                Nivel2 = request.Nivel2,
                Nivel3 = request.Nivel3,
                Copia = NormalizarTexto(request.Copia),
                DescripcionSubCatOcu = NormalizarTexto(
                    request.DescripcionSubCatOcu),
                DatosAdjuntos = request.DatosAdjuntos,
                CopiaJefatura = request.CopiaJefatura
            }, cancellationToken).ConfigureAwait(false);

            return MapearRespuesta(registro);
        }

        public async Task<CatSubOcuResponse> ActualizarAsync(
            int id,
            ActualizarCatSubOcuRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "El identificador debe ser mayor que cero.", "id");
            }

            if (request == null)
            {
                throw new ArgumentException(
                    "El cuerpo de la solicitud es obligatorio.", "request");
            }

            ValidarConfiguracion(
                request.IdTipoRegistro,
                request.IdCategoria,
                request.IdSubcategoria,
                request.IdOcurrencia,
                request.NivelAprobacion,
                request.RolAprobacion,
                request.Copia);

            var registro = await repository.ActualizarAsync(
                id,
                new Entities.CatSubOcu
                {
                    IdTipoRegistro = request.IdTipoRegistro,
                    IdCategoria = request.IdCategoria,
                    IdSubcategoria = request.IdSubcategoria,
                    IdOcurrencia = request.IdOcurrencia,
                    RequiereAprobacion = request.RequiereAprobacion,
                    NivelAprobacion = NormalizarTexto(request.NivelAprobacion),
                    RolAprobacion = NormalizarTexto(request.RolAprobacion),
                    TiempoAtencion = request.TiempoAtencion,
                    Estado = request.Estado,
                    Atencion = request.Atencion,
                    Nivel1 = request.Nivel1,
                    Nivel2 = request.Nivel2,
                    Nivel3 = request.Nivel3,
                    Copia = NormalizarTexto(request.Copia),
                    DescripcionSubCatOcu = NormalizarTexto(
                        request.DescripcionSubCatOcu),
                    DatosAdjuntos = request.DatosAdjuntos,
                    CopiaJefatura = request.CopiaJefatura
                },
                cancellationToken).ConfigureAwait(false);

            return registro == null ? null : MapearRespuesta(registro);
        }

        public async Task<IList<CatSubOcuResponse>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var registros = await repository.ObtenerTodosAsync(
                cancellationToken).ConfigureAwait(false);

            return registros.Select(registro => new CatSubOcuResponse
            {
                Id = registro.Id,
                IdTipoRegistro = registro.IdTipoRegistro,
                IdCategoria = registro.IdCategoria,
                IdSubcategoria = registro.IdSubcategoria,
                IdOcurrencia = registro.IdOcurrencia,
                TipoRegistro = registro.TipoRegistro,
                Categoria = registro.Categoria,
                Subcategoria = registro.Subcategoria,
                Ocurrencia = registro.Ocurrencia,
                RequiereAprobacion = registro.RequiereAprobacion,
                NivelAprobacion = registro.NivelAprobacion,
                RolAprobacion = registro.RolAprobacion,
                TiempoAtencion = registro.TiempoAtencion,
                Estado = registro.Estado,
                Atencion = registro.Atencion,
                Nivel1 = registro.Nivel1,
                Nivel2 = registro.Nivel2,
                Nivel3 = registro.Nivel3,
                Copia = registro.Copia,
                DescripcionSubCatOcu = registro.DescripcionSubCatOcu,
                DatosAdjuntos = registro.DatosAdjuntos,
                CopiaJefatura = registro.CopiaJefatura
            }).ToList();
        }

        public async Task<CatSubOcuPorIdResponse> ObtenerPorIdAsync(
            int id,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "El identificador debe ser mayor que cero.", "id");
            }

            var registro = await repository.ObtenerPorIdAsync(
                id, cancellationToken).ConfigureAwait(false);

            return registro == null ? null : MapearRespuestaPorId(registro);
        }

        public async Task<IList<CatSubOcuPorOcurrenciaResponse>>
            ObtenerPorOcurrenciaAsync(
                int idOcurrencia,
                CancellationToken cancellationToken = default(CancellationToken))
        {
            if (idOcurrencia <= 0)
            {
                throw new ArgumentException(
                    "El idOcurrencia debe ser mayor que cero.",
                    "idOcurrencia");
            }

            var registros = await repository.ObtenerPorOcurrenciaAsync(
                idOcurrencia,
                cancellationToken).ConfigureAwait(false);

            return registros.Select(registro =>
                new CatSubOcuPorOcurrenciaResponse
                {
                    Copia = registro.Copia,
                    DatosAdjuntos = registro.DatosAdjuntos,
                    CopiaJefatura = registro.CopiaJefatura,
                    DescripcionCatSubOcu = registro.DescripcionSubCatOcu
                }).ToList();
        }

        private static string NormalizarTexto(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private static void ValidarConfiguracion(
            int idTipoRegistro,
            int idCategoria,
            int idSubcategoria,
            int idOcurrencia,
            string nivelAprobacion,
            string rolAprobacion,
            string copia)
        {
            if (idTipoRegistro <= 0
                || idCategoria <= 0
                || idSubcategoria <= 0
                || idOcurrencia <= 0)
            {
                throw new ArgumentException(
                    "Los identificadores de tipo, categorÃ­a, subcategorÃ­a y ocurrencia deben ser mayores que cero.",
                    "request");
            }

            if (!string.IsNullOrWhiteSpace(nivelAprobacion)
                && nivelAprobacion.Trim().Length > 5)
            {
                throw new ArgumentException(
                    "NivelAprobacion no puede superar los 5 caracteres.",
                    "NivelAprobacion");
            }

            if (!string.IsNullOrWhiteSpace(rolAprobacion)
                && rolAprobacion.Trim().Length > 10)
            {
                throw new ArgumentException(
                    "RolAprobacion no puede superar los 10 caracteres.",
                    "RolAprobacion");
            }

            if (!string.IsNullOrWhiteSpace(copia)
                && copia.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "Copia no puede superar los 100 caracteres.",
                    "Copia");
            }
        }

        private static CatSubOcuResponse MapearRespuesta(
            Entities.CatSubOcu registro)
        {
            return new CatSubOcuResponse
            {
                Id = registro.Id,
                IdTipoRegistro = registro.IdTipoRegistro,
                IdCategoria = registro.IdCategoria,
                IdSubcategoria = registro.IdSubcategoria,
                IdOcurrencia = registro.IdOcurrencia,
                TipoRegistro = registro.TipoRegistro,
                Categoria = registro.Categoria,
                Subcategoria = registro.Subcategoria,
                Ocurrencia = registro.Ocurrencia,
                RequiereAprobacion = registro.RequiereAprobacion,
                NivelAprobacion = registro.NivelAprobacion,
                RolAprobacion = registro.RolAprobacion,
                TiempoAtencion = registro.TiempoAtencion,
                Estado = registro.Estado,
                Atencion = registro.Atencion,
                Nivel1 = registro.Nivel1,
                Nivel2 = registro.Nivel2,
                Nivel3 = registro.Nivel3,
                Copia = registro.Copia,
                DescripcionSubCatOcu = registro.DescripcionSubCatOcu,
                DatosAdjuntos = registro.DatosAdjuntos,
                CopiaJefatura = registro.CopiaJefatura
            };
        }

        private static CatSubOcuPorIdResponse MapearRespuestaPorId(
            Entities.CatSubOcu registro)
        {
            return new CatSubOcuPorIdResponse
            {
                Id = registro.Id,
                IdTipoRegistro = registro.IdTipoRegistro,
                IdCategoria = registro.IdCategoria,
                IdSubcategoria = registro.IdSubcategoria,
                IdOcurrencia = registro.IdOcurrencia,
                RequiereAprobacion = registro.RequiereAprobacion,
                NivelAprobacion = registro.NivelAprobacion,
                RolAprobacion = registro.RolAprobacion,
                TiempoAtencion = registro.TiempoAtencion,
                Estado = registro.Estado,
                Atencion = registro.Atencion,
                Nivel1 = registro.Nivel1,
                Nivel2 = registro.Nivel2,
                Nivel3 = registro.Nivel3,
                Copia = registro.Copia,
                DescripcionSubCatOcu = registro.DescripcionSubCatOcu,
                DatosAdjuntos = registro.DatosAdjuntos,
                CopiaJefatura = registro.CopiaJefatura
            };
        }
    }
}
