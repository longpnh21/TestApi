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

        public virtual async Task<IEnumerable<TEntity>> GetWithPaginationAsync(
           int pageIndex = 1,
           int pageSize = 10,
           Expression<Func<TEntity, bool>> filter = null,
           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
           string includeProperties = "",
           bool isDelete = false)
        {
            IQueryable<TEntity> query = _dbSet;

            if (!isDelete)
            {
                query = query.Where(e => !e.IsDelete);
            }

            if (filter is not null)
            {
                query = query.Where(filter);
            }

            foreach (string includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            return orderBy is not null
                ? await orderBy(query)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking().ToListAsync()
                : (IEnumerable<TEntity>)await query
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking().ToListAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(
           Expression<Func<TEntity, bool>> filter = null,
           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
           string includeProperties = "",
           bool isDelete = false)
        {
            IQueryable<TEntity> query = _dbSet;

            if (!isDelete)
            {
                query = query.Where(e => !e.IsDelete);
            }

            if (filter is not null)
            {
                query = query.Where(filter);
            }

            foreach (string includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            return orderBy is not null
                ? await orderBy(query)
                    .AsNoTracking().ToListAsync()
                : (IEnumerable<TEntity>)await query
                    .AsNoTracking().ToListAsync();
        }

        public virtual async Task<TEntity> GetByIdAsync(object id)
            => await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

        public virtual Task InsertAsync(TEntity entity)
        {
            _dbSet.Add(entity);
            return Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(object id)
        {
            var entityToDelete = await GetByIdAsync(id);
            if (entityToDelete is null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            await DeleteAsync(entityToDelete);
        }

        public virtual async Task HardDeleteAsync(object id)
        {
            var entityToDelete = await GetByIdAsync(id);
            if (entityToDelete is null)
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
