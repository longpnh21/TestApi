using Microsoft.EntityFrameworkCore;
using Project.Core.Entities;
using Project.Infrastructure.Common;
using System.Threading.Tasks;

namespace Project.Infrastructure.Repositories
{
    public class LocationRepository : BaseRepository<Location>, ILocationRepository
    {
        public LocationRepository(IApplicationDbContext context) : base(context)
        {
        }

        public override async Task InsertAsync(Location entity)
        {
            entity.Id = await _dbSet.MaxAsync(e => e.Id) + 1;
            _dbSet.Add(entity);
        }
    }
}
