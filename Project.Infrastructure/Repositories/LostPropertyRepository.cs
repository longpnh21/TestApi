using Microsoft.EntityFrameworkCore;
using Project.Core.Entities;
using Project.Infrastructure.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Infrastructure.Repositories
{
    public class LostPropertyRepository : BaseRepository<LostProperty>, ILostPropertyRepository
    {
        public LostPropertyRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.LostProperties;
        }

        public virtual async Task HardDeleteAsync(IEnumerable<LostProperty> entitiesToDelete)
        {
            foreach (var entity in entitiesToDelete)
            {
                _dbSet.Remove(entity);
            }
        }

        public override async Task InsertAsync(LostProperty entity)
        {
            entity.Id = await _dbSet.MaxAsync(e => e.Id) + 1;
            _dbSet.Add(entity);
        }
    }
}
