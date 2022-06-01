using Project.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Services
{
    public interface ILostPropertyService
    {
        Task AddAsync(LostProperty lostProperty);
        Task DeleteAsync(int id);
        Task<IEnumerable<LostProperty>> GetAllAsync();
        Task<LostProperty> GetByIdAsync(int id);
        Task HardDeleteAsync(int id);
        Task UpdateAsync(LostProperty lostProperty);
    }
}