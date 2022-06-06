using Project.Core.Common;
using Project.Core.Entities;
using Project.Infrastructure.Common;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Project.Services
{
    public class LocationService : ILocationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LocationService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task AddAsync(Location location)
        {
            await _unitOfWork.LocationRepository.InsertAsync(location);
            await _unitOfWork.SaveAsync();
            _unitOfWork.Dispose();
        }

        public async Task DeleteAsync(int id)
        {
            await _unitOfWork.LocationRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            _unitOfWork.Dispose();
        }

        public async Task HardDeleteAsync(int id)
        {
            await _unitOfWork.LocationRepository.HardDeleteAsync(id);
            await _unitOfWork.SaveAsync();
            _unitOfWork.Dispose();
        }

        public async Task<PaginatedList<Location>> GetAllAsync(
           int pageIndex = 1,
           int pageSize = 10,
           Expression<Func<Location, bool>> filter = null,
           Func<IQueryable<Location>, IOrderedQueryable<Location>> orderBy = null,
           string includeProperties = "",
           bool isDelete = false) => await _unitOfWork.LocationRepository.GetWithPaginationAsync(
                pageIndex,
                pageSize,
                filter,
                orderBy,
                includeProperties,
                isDelete);

        public async Task<Location> GetByIdAsync(int id) => await _unitOfWork.LocationRepository.GetByIdAsync(id);

        public async Task UpdateAsync(Location location)
        {
            await _unitOfWork.LocationRepository.UpdateAsync(location);
            await _unitOfWork.SaveAsync();
            _unitOfWork.Dispose();
        }
    }
}
