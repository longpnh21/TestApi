using Project.Core.Common;
using Project.Core.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Project.Services
{
    public interface ILostPropertyService
    {
        Task AddAsync(LostProperty lostProperty);
        Task DeleteAsync(int id);
        Task<PaginatedList<LostProperty>> GetAllAsync(
           int pageIndex = 1,
           int pageSize = 10,
           Expression<Func<LostProperty, bool>> filter = null,
           Func<IQueryable<LostProperty>, IOrderedQueryable<LostProperty>> orderBy = null,
           string includeProperties = "",
           bool isDelete = false);
        Task<LostProperty> GetByIdAsync(int id);
        Task HardDeleteAsync(int id);
        Task UpdateAsync(LostProperty lostProperty);
    }
}