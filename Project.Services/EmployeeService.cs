using Project.Core.Common;
using Project.Core.Entities;
using Project.Infrastructure.Common;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Project.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmployeeService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task AddAsync(Employee employee)
        {
            await _unitOfWork.EmployeeRepository.InsertAsync(employee);
            await _unitOfWork.SaveAsync();
            _unitOfWork.Dispose();
        }

        public async Task DeleteAsync(string id)
        {
            await _unitOfWork.EmployeeRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            _unitOfWork.Dispose();
        }

        public async Task HardDeleteAsync(string id)
        {
            var employeeLostProperties = await _unitOfWork.LostPropertyRepository.GetAllAsync(filter: e => e.EmployeeId == id);
            if (employeeLostProperties is not null)
            {
                await _unitOfWork.LostPropertyRepository.HardDeleteAsync(employeeLostProperties.Result);
            }
            await _unitOfWork.EmployeeRepository.HardDeleteAsync(id);
            await _unitOfWork.SaveAsync();
            _unitOfWork.Dispose();
        }

        public async Task<PaginatedList<Employee>> GetAsync(
           int pageIndex = 1,
           int pageSize = 10,
           Expression<Func<Employee, bool>> filter = null,
           Func<IQueryable<Employee>, IOrderedQueryable<Employee>> orderBy = null,
           string includeProperties = "",
           bool isDelete = false) => await _unitOfWork.EmployeeRepository.GetWithPaginationAsync(
                pageIndex,
                pageSize,
                filter,
                orderBy,
                includeProperties,
                isDelete);

        public async Task<Employee> GetByIdAsync(string id) => await _unitOfWork.EmployeeRepository.GetByIdAsync(id);

        public async Task UpdateAsync(Employee employee)
        {
            await _unitOfWork.EmployeeRepository.UpdateAsync(employee);
            await _unitOfWork.SaveAsync();
            _unitOfWork.Dispose();
        }
    }
}
