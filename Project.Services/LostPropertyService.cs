using Project.Core.Entities;
using Project.Infrastructure.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Services
{
    public class LostPropertyService : ILostPropertyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LostPropertyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddAsync(LostProperty lostProperty)
        {
            if (!string.IsNullOrEmpty(lostProperty.EmployeeId))
            {
                var employee = await _unitOfWork.EmployeeRepository.GetByIdAsync(lostProperty.Id);
                if (employee == null)
                {
                    throw new ArgumentNullException(nameof(lostProperty.EmployeeId));
                }
            }
            await _unitOfWork.LostPropertyRepository.InsertAsync(lostProperty);
            await _unitOfWork.SaveAsync();
            _unitOfWork.Dispose();
        }

        public async Task DeleteAsync(int id)
        {
            await _unitOfWork.LostPropertyRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            _unitOfWork.Dispose();
        }

        public async Task HardDeleteAsync(int id)
        {
            await _unitOfWork.LostPropertyRepository.HardDeleteAsync(id);
            await _unitOfWork.SaveAsync();
            _unitOfWork.Dispose();
        }

        public async Task<IEnumerable<LostProperty>> GetAllAsync()
        {
            return await _unitOfWork.LostPropertyRepository.GetAsync();
        }

        public async Task<LostProperty> GetByIdAsync(int id)
        {
            return await _unitOfWork.LostPropertyRepository.GetByIdAsync(id);
        }

        public async Task UpdateAsync(LostProperty lostProperty)
        {
            if (!string.IsNullOrEmpty(lostProperty.EmployeeId))
            {
                var employee = await _unitOfWork.EmployeeRepository.GetByIdAsync(lostProperty.Id);
                if (employee == null)
                {
                    throw new ArgumentNullException(nameof(lostProperty.EmployeeId));
                }
            }

            await _unitOfWork.LostPropertyRepository.UpdateAsync(lostProperty);
            await _unitOfWork.SaveAsync();
            _unitOfWork.Dispose();
        }
    }
}
