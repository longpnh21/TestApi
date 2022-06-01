using Project.Core.Entities;
using System.Collections.Generic;
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
        Task<IEnumerable<Employee>> GetAllAsync();
    }
}
