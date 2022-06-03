using Project.Infrastructure.Repositories;
using System;
using System.Threading.Tasks;

namespace Project.Infrastructure.Common
{
    public class UnitOfWork : IDisposable, IUnitOfWork
    {
        private readonly IApplicationDbContext _context;
        private IEmployeeRepository _employeeRepository;
        private ILostPropertyRepository _lostPropertyRepository;
        private ILocationRepository _locationRepository;

        public UnitOfWork(IApplicationDbContext context)
        {
            _context = context;
        }

        public IEmployeeRepository EmployeeRepository
        {
            get
            {

                if (_employeeRepository is null)
                {
                    _employeeRepository = new EmployeeRepository(_context);
                }
                return _employeeRepository;
            }
        }

        public ILostPropertyRepository LostPropertyRepository
        {
            get
            {

                if (_lostPropertyRepository is null)
                {
                    _lostPropertyRepository = new LostPropertyRepository(_context);
                }
                return _lostPropertyRepository;
            }
        }

        public ILocationRepository LocationRepository
        {
            get
            {

                if (_locationRepository is null)
                {
                    _locationRepository = new LocationRepository(_context);
                }
                return _locationRepository;
            }
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
