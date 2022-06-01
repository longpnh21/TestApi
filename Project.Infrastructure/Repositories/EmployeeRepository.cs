using Project.Core.Entities;
using Project.Infrastructure.Common;

namespace Project.Infrastructure.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.Employees;
        }
    }
}
