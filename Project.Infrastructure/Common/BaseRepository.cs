using Microsoft.EntityFrameworkCore;
using Project.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Project.Infrastructure.Common
{
    public class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {

        protected IApplicationDbContext _context;
        protected DbSet<TEntity> _dbSet;

        public BaseRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public virtual async Task<IEnumerable<TEntity>> GetAsync(
           Expression<Func<TEntity, bool>> filter = null,
           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
           string includeProperties = "")
        {
            IQueryable<TEntity> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (string includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return await orderBy(query).AsNoTracking().ToListAsync();
            }
            else
            {
                return await query.AsNoTracking().ToListAsync();
            }
        }

        public virtual async Task<TEntity> GetByIdAsync(object id)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id.Equals(id));
        }

        public virtual Task InsertAsync(TEntity entity)
        {
            _dbSet.Add(entity);
            return Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(object id)
        {
            TEntity entityToDelete = await GetByIdAsync(id);
            if (entityToDelete == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            await DeleteAsync(entityToDelete);
        }

        public virtual async Task HardDeleteAsync(object id)
        {
            TEntity entityToDelete = await GetByIdAsync(id);
            if (entityToDelete == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            await HardDeleteAsync(entityToDelete);
        }

        public virtual Task DeleteAsync(TEntity entityToDelete)
        {
            if (_context.IsEntityDetached(entityToDelete))
            {
                _dbSet.Attach(entityToDelete);
            }
            _dbSet.Remove(entityToDelete);
            return Task.CompletedTask;
        }

        public virtual Task HardDeleteAsync(TEntity entityToDelete)
        {
            if (_context.IsEntityDetached(entityToDelete))
            {
                _dbSet.Attach(entityToDelete);
            }
            entityToDelete.IsSoftDelete = false;
            _dbSet.Remove(entityToDelete);
            return Task.CompletedTask;
        }

        public virtual Task UpdateAsync(TEntity entityToUpdate)
        {
            _dbSet.Attach(entityToUpdate);
            _context.MarkAsModified(entityToUpdate);
            return Task.CompletedTask;
        }
    }
}
