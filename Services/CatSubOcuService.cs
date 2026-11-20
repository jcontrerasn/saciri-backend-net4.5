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
                Copia = request.Copia,
                DescripcionSubCatOcu = NormalizarTexto(
                    request.DescripcionSubCatOcu),
                DatosAdjuntos = request.DatosAdjuntos,
                CopiaJefatura = request.CopiaJefatura
            }, cancellationToken).ConfigureAwait(false);

            return MapearRespuesta(registro);
        }

        public async Task<IList<CatSubOcuResponse>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var registros = await repository.ObtenerTodosAsync(
                cancellationToken).ConfigureAwait(false);

            return registros.Select(registro => new CatSubOcuResponse
            {
                Id = registro.Id,
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

        private static string NormalizarTexto(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private static CatSubOcuResponse MapearRespuesta(
            Entities.CatSubOcu registro)
        {
            return new CatSubOcuResponse
            {
                Id = registro.Id,
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
    }
}
