using Project.Core.Common;
using Project.Core.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Project.Services
{
    public interface ILocationService
    {
        Task AddAsync(Location lostProperty);
        Task DeleteAsync(int id);
        Task<PaginatedList<Location>> GetAllAsync(
           int pageIndex = 1,
           int pageSize = 10,
           Expression<Func<Location, bool>> filter = null,
           Func<IQueryable<Location>, IOrderedQueryable<Location>> orderBy = null,
           string includeProperties = "",
           bool isDelete = false);
        Task<Location> GetByIdAsync(int id);
        Task HardDeleteAsync(int id);
        Task UpdateAsync(Location lostProperty);
    }
}
