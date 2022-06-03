using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Project.Core.Entities;
using Project.Infrastructure.Common;
using Project.Infrastructure.Repositories;
using Project.Services;
using Project.Test.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Project.Tests.ServicesTests
{
    [TestFixture]
    public class LocationServiceTests
    {
        #region Variables
        private ILocationService _locationService;
        private IUnitOfWork _unitOfWork;
        private List<Location> _locations;
        private ILocationRepository _locationRepository;
        private IApplicationDbContext _applicationDbContext;
        #endregion

        #region Test fixture setup
        [OneTimeSetUp]
        public void SetUp()
        {
            _locations = DataInitializer.GetAllLocations();
        }
        #endregion

        #region Setup
        [SetUp]
        public void ReInitializeTest()
        {
            _locations = DataInitializer.GetAllLocations();
            _applicationDbContext = new Mock<IApplicationDbContext>().Object;
            _locationRepository = SetUpLocationRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.LocationRepository).Returns(_locationRepository);
            _unitOfWork = unitOfWork.Object;
            _locationService = new LocationService(unitOfWork.Object);
        }
        #endregion

        #region Unit Tests
        [Test]
        [TestCase(1, 2)]
        public async Task GetAllAsync_ReturnCollection(int pageIndex, int pageSize)
        {
            var locations = await _locationService.GetAllAsync(pageIndex, pageSize);
            CollectionAssert.AreEqual(locations, _locations.Skip((pageIndex - 1) * pageSize).Take(pageSize));
        }

        [Test]
        [TestCase(6)]
        public async Task GetByIdAsync_ReturnLocation(int id)
        {
            var location = await _locationService.GetByIdAsync(id);
            Assert.AreEqual(_locations[id], location);
        }

        [Test]
        [TestCase(-1)]
        public async Task GetByIdAsync_ReturnNull(int id)
        {
            var location = await _locationService.GetByIdAsync(id);
            Assert.Null(location);
        }


        [Test]
        public async Task AddAsync()
        {
            var insertedLocation = new Location
            {
                Floor = 6,
                Cube = "FlyTeam"
            };

            await _locationService.AddAsync(insertedLocation);
            Assert.AreEqual(_locations.Last(), insertedLocation);
        }

        [Test]
        public async Task UpdateAsync()
        {
            var location = new Location
            {
                Id = 4
            };

            var updatedLocation = new Location
            {
                Id = 4,
                Floor = 0,
                Cube = "Test"
            };

            await _locationService.UpdateAsync(updatedLocation);
            var changedLocation = _locations.FirstOrDefault(x => x.Id == location.Id);
            Assert.True(changedLocation.Floor == 0
                && changedLocation.Cube == "Test");
        }

        [Test]
        [TestCase("8")]
        public async Task DeleteAsync(int id)
        {

            await _locationService.DeleteAsync(id);
            var deletedLocation = _locations.FirstOrDefault(x => x.Id == id);
            Assert.True(deletedLocation.IsDelete);
        }

        [Test]
        [TestCase("8")]
        public async Task HardDeleteAsync(int id)
        {

            var preDeletedLocation = _locations.FirstOrDefault(e => e.Id == id);
            await _locationService.HardDeleteAsync(id);
            var deletedLocation = _locations.FirstOrDefault(x => x.Id == id);
            Assert.True((preDeletedLocation is not null)
                && (deletedLocation is null));
        }
        #endregion

        #region Private member methods


        private ILocationRepository SetUpLocationRepository()
        {
            var mockRepo = new Mock<LocationRepository>(MockBehavior.Default, _applicationDbContext);

            mockRepo.Setup(x => x.GetWithPaginationAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Location, bool>>>(),
                It.IsAny<Func<IQueryable<Location>, IOrderedQueryable<Location>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(
                    (int pageIndex,
                    int pageSize,
                    Expression<Func<Location, bool>> filter,
                    Func<IQueryable<Location>, IOrderedQueryable<Location>> orderBy,
                    string includeProperties,
                    bool isDelete
                    ) =>
                    {
                        var query = _locations.AsQueryable();

                        if (!isDelete)
                        {
                            query = query.Where(e => !e.IsDelete);
                        }

                        if (filter is not null)
                        {
                            query = query.Where(filter);
                        }

                        foreach (string includeProperty in includeProperties.Split
                            (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            query = query.Include(includeProperty);
                        }

                        return orderBy is not null
                            ? orderBy(query)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                            : (IEnumerable<Location>)query
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize);
                    }
                );

            mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Func<int, Location>(id => _locations.FirstOrDefault(e => e.Id == id)));

            mockRepo.Setup(p => p.InsertAsync((It.IsAny<Location>())))
                .Callback(new Action<Location>(newLocation =>
                {
                    int locationId = _locations.Max(e => e.Id);
                    newLocation.Id = locationId + 1;
                    _locations.Add(newLocation);
                }));

            mockRepo.Setup(x => x.UpdateAsync(It.IsAny<Location>()))
                .Callback(new Action<Location>(emp =>
                {
                    int oldLocation = _locations.FindIndex(e => e.Id == emp.Id);
                    _locations[oldLocation] = emp;
                }));

            mockRepo.Setup(x => x.DeleteAsync(It.IsAny<object>()))
                .Callback((object locationId) =>
                {
                    int id = int.Parse(locationId.ToString());
                    var locationToRemove = _locations.FirstOrDefault(e => e.Id == id);
                    if (locationToRemove is not null)
                    {
                        locationToRemove.IsDelete = true;
                    }
                });


            mockRepo.Setup(x => x.DeleteAsync(It.IsAny<Location>()))
                .Callback(new Action<Location>(emp =>
                {
                    var locationToRemove = _locations.FirstOrDefault(e => e.Id == emp.Id);
                    if (locationToRemove is not null)
                    {
                        locationToRemove.IsDelete = true;
                    }
                }));

            mockRepo.Setup(x => x.HardDeleteAsync(It.IsAny<object>()))
                .Callback((object locationId) =>
                {
                    int id = int.Parse(locationId.ToString());
                    var locationToRemove = _locations.FirstOrDefault(e => e.Id == id);
                    if (locationToRemove is not null)
                    {
                        _locations.Remove(locationToRemove);
                    }
                });

            mockRepo.Setup(x => x.HardDeleteAsync(It.IsAny<Location>()))
                .Callback(new Action<Location>(emp =>
                {
                    var locationToRemove = _locations.FirstOrDefault(e => e.Id == emp.Id);
                    if (locationToRemove is not null)
                    {
                        _locations.Remove(locationToRemove);
                    }
                }));

            return mockRepo.Object;
        }
        #endregion

        #region TearDown
        [TearDown]
        public void DisposeTest()
        {
            _locationService = null;
            _unitOfWork = null;
            _locationRepository = null;
            if (_applicationDbContext is not null)
            {
                _applicationDbContext.Dispose();
            }
            _locations = null;
        }
        #endregion

        #region TestFixture TearDown
        [OneTimeTearDown]
        public void DisposeAllObjects()
        {
            _locations = null;
        }
        #endregion
    }
}
