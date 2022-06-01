using Project.Core.Entities;
using Project.Infrastructure.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Infrastructure.Repositories
{
    public interface ILostPropertyRepository : IRepository<LostProperty>
    {
        Task HardDeleteAsync(IEnumerable<LostProperty> entitiesToDelete);
    }
}
