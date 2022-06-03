using Project.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Project.Infrastructure.Common
{
    public interface IRepository<TEntity> where TEntity : BaseEntity
    {
        Task<IEnumerable<TEntity>> GetWithPaginationAsync(
           int pageIndex = 1,
           int pageSize = 10,
           Expression<Func<TEntity, bool>> filter = null,
           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
           string includeProperties = "",
           bool isDelete = false);

        Task<IEnumerable<TEntity>> GetAllAsync(
           Expression<Func<TEntity, bool>> filter = null,
           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
           string includeProperties = "",
           bool isDelete = false);

        Task<TEntity> GetByIdAsync(object id);

        Task InsertAsync(TEntity entity);

        Task DeleteAsync(object id);

        Task DeleteAsync(TEntity entityToDelete);

        Task HardDeleteAsync(object id);

        Task HardDeleteAsync(TEntity entityToDelete);

        Task UpdateAsync(TEntity entityToUpdate);
    }
}
