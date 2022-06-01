using Project.Infrastructure.Repositories;
using System.Threading.Tasks;

namespace Project.Infrastructure.Common
{
    public interface IUnitOfWork
    {
        IEmployeeRepository EmployeeRepository { get; }
        ILostPropertyRepository LostPropertyRepository { get; }

        void Dispose();
        Task SaveAsync();
    }
}