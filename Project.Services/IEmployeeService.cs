using Project.Core.Common;
using Project.Core.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Project.Services
{
    public interface IEmployeeService
    {
        Task AddAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task<Employee> GetByIdAsync(string id);
        Task DeleteAsync(string id);
        Task HardDeleteAsync(string id);
        Task<PaginatedList<Employee>> GetAsync(
           int pageIndex = 1,
           int pageSize = 10,
           Expression<Func<Employee, bool>> filter = null,
           Func<IQueryable<Employee>, IOrderedQueryable<Employee>> orderBy = null,
           string includeProperties = "",
           bool isDelete = false);
    }
}
