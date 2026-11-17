using System.Collections.Generic;
using SACIRI_backend.Entities;

namespace SACIRI_backend.Interfaces
{
    public interface IRepository<TEntity> where TEntity : EntityBase
    {
        IEnumerable<TEntity> GetAll();

        TEntity GetById(int id);

        TEntity Add(TEntity entity);
    }
}
