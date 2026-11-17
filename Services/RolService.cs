using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SACIRI_backend.Dtos;
using SACIRI_backend.Interfaces;

namespace SACIRI_backend.Services
{
    public class RolService
    {
        private readonly IRolRepository repository;

        public RolService(IRolRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.repository = repository;
        }

        public async Task<IList<RolResponse>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var roles = await repository.ObtenerTodosAsync(
                cancellationToken).ConfigureAwait(false);

            return roles.Select(rol => new RolResponse
            {
                IdRol = rol.IdRol,
                Nombre = rol.Nombre,
                Descripcion = rol.Descripcion,
                Activo = rol.Activo,
                FechaRegistro = rol.FechaRegistro
            }).ToList();
        }
    }
}
