using Project.Core.Common;
using Project.Core.Entities;
using Project.Infrastructure.Common;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Project.Services
{
    public class LostPropertyService : ILostPropertyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LostPropertyService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task AddAsync(LostProperty lostProperty)
        {
            if (!string.IsNullOrWhiteSpace(lostProperty.EmployeeId))
            {
                var employee = await _unitOfWork.EmployeeRepository.GetByIdAsync(lostProperty.EmployeeId);
                if (employee is null)
                {
                    throw new ArgumentNullException(nameof(lostProperty.EmployeeId));
                }
            }
            if (lostProperty.LocationId != null)
            {
                var location = await _unitOfWork.LocationRepository.GetByIdAsync(lostProperty.LocationId);
                if (location is null)
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

        public async Task<PaginatedList<LostProperty>> GetAllAsync(
           int pageIndex = 1,
           int pageSize = 10,
           Expression<Func<LostProperty, bool>> filter = null,
           Func<IQueryable<LostProperty>, IOrderedQueryable<LostProperty>> orderBy = null,
           string includeProperties = "",
           bool isDelete = false) => await _unitOfWork.LostPropertyRepository.GetWithPaginationAsync(
                pageIndex,
                pageSize,
                filter,
                orderBy,
                includeProperties,
                isDelete);

        public async Task<LostProperty> GetByIdAsync(int id) => await _unitOfWork.LostPropertyRepository.GetByIdAsync(id);

        public async Task UpdateAsync(LostProperty lostProperty)
        {
            if (!string.IsNullOrWhiteSpace(lostProperty.EmployeeId))
            {
                var employee = await _unitOfWork.EmployeeRepository.GetByIdAsync(lostProperty.EmployeeId);
                if (employee is null)
                {
                    throw new ArgumentNullException(nameof(lostProperty.EmployeeId));
                }
            }
            if (lostProperty.LocationId != null)
            {
                var location = await _unitOfWork.LocationRepository.GetByIdAsync(lostProperty.LocationId);
                if (location is null)
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
