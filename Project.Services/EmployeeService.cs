using Project.Core.Entities;
using Project.Infrastructure.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmployeeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

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
            var employeeLostProperties = await _unitOfWork.LostPropertyRepository.GetAsync(e => e.EmployeeId.Equals(id));
            if (employeeLostProperties != null)
            {
                await _unitOfWork.LostPropertyRepository.HardDeleteAsync(employeeLostProperties);
            }
            await _unitOfWork.EmployeeRepository.HardDeleteAsync(id);
            await _unitOfWork.SaveAsync();
            _unitOfWork.Dispose();
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _unitOfWork.EmployeeRepository.GetAsync();
        }

        public async Task<Employee> GetByIdAsync(string id)
        {
            return await _unitOfWork.EmployeeRepository.GetByIdAsync(id);
        }

        public async Task UpdateAsync(Employee employee)
        {
            await _unitOfWork.EmployeeRepository.UpdateAsync(employee);
            await _unitOfWork.SaveAsync();
            _unitOfWork.Dispose();
        }
    }
}
