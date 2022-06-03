using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Project.Api.Controllers;
using Project.Application.AutoMapper;
using Project.Application.Dtos.Location;
using Project.Core.Entities;
using Project.Services;
using Project.Test.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Project.Tests.ControllersTests
{
    public class LocationsControllerTests
    {
        #region Variables
        private ILocationService _locationService;
        private List<Location> _locations;
        private List<LostProperty> _lostProperties;
        private IMapper _mapper;
        private LocationsController _locationsController;
        #endregion

        #region Test fixture setup
        [SetUp]
        public void Setup()
        {
            _locations = DataInitializer.GetAllLocations();
            _lostProperties = DataInitializer.GetAllLostProperties();
            _locationService = SetUpLocationService();
            _mapper = SetupAutoMapper();
            _locationsController = new LocationsController(_locationService, _mapper);
        }

        #endregion

        #region Setup
        [OneTimeSetUp]
        public void ReInitializeTest()
        {
            _locations = DataInitializer.GetAllLocations();
        }
        #endregion

        #region Private member methods

        private ILocationService SetUpLocationService()
        {
            var mockService = new Mock<ILocationService>();

            mockService.Setup(x => x.AddAsync(It.IsAny<Location>()))
                .Callback(new Action<Location>(newLocation =>
                {
                    int maxId = _locations.Max(e => e.Id);
                    newLocation.Id = maxId++;
                    _locations.Add(newLocation);
                }));
            mockService.Setup(x => x.UpdateAsync(It.IsAny<Location>()))
                .Callback(new Action<Location>(updatedLocation =>
                {
                    int oldLocation = _locations.FindIndex(e => e.Id == updatedLocation.Id);
                    _locations[oldLocation] = updatedLocation;
                }));
            mockService.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Func<int, Location>(id => _locations.FirstOrDefault(e => e.Id == id)));

            mockService.Setup(x => x.DeleteAsync(It.IsAny<int>()))
                .Callback((int id) =>
                {
                    var locationToRemove = _locations.FirstOrDefault(e => e.Id == id);
                    if (locationToRemove is null)
                    {
                        throw new ArgumentNullException();
                    }
                    locationToRemove.IsDelete = true;
                });
            mockService.Setup(x => x.HardDeleteAsync(It.IsAny<int>()))
                .Callback((int id) =>
                {
                    var locationToRemove = _locations.FirstOrDefault(e => e.Id == id);
                    if (locationToRemove is not null)
                    {
                        var locationProperties = _lostProperties.Where(p => id == p.LocationId).ToList();
                        var temp = new List<LostProperty>();
                        temp = locationProperties;
                        foreach (var property in temp)
                        {
                            _lostProperties.Remove(property);
                        }

                        _locations.Remove(locationToRemove);
                    }
                });

            mockService.Setup(x => x.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Location, bool>>>(),
                It.IsAny<Func<IQueryable<Location>, IOrderedQueryable<Location>>>(),
                It.IsAny<string>()))
                .ReturnsAsync(
                    (int pageIndex,
                    int pageSize,
                    Expression<Func<Location, bool>> filter,
                    Func<IQueryable<Location>, IOrderedQueryable<Location>> orderBy,
                    string includeProperties
                    ) =>
                    {
                        var query = _locations.AsQueryable();

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
                                .Take(pageSize).ToList()
                            : (IEnumerable<Location>)query
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize).ToList();
                    }
                );

            return mockService.Object;
        }

        private IMapper SetupAutoMapper()
        {
            return new MapperConfiguration(cfg => cfg.AddProfile<ApplicationProfile>()).CreateMapper();
        }
        #endregion

        #region Unit tests
        [Test]
        [TestCase(1, 3)]
        public async Task GetAsync_Return200Ok(int pageIndex = 1, int pageSize = 10)
        {

            var response = await _locationsController.GetAsync(pageIndex, pageSize) as OkObjectResult;
            var responseObject = response.Value as IEnumerable<LocationDto>;
            var expected = _locations.Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(e => _mapper.Map<LocationDto>(e)).ToList();
            Assert.AreEqual(200, response.StatusCode);
            Assert.AreEqual(expected.Count, responseObject.ToList().Count);
        }

        [Test]
        [TestCase(10)]
        public async Task GetByIdAsync_Return200Ok(int id)
        {
            var expectedResult = _mapper.Map<LocationDto>(_locations.FirstOrDefault(e => e.Id == id));

            var response = await _locationsController.GetByIdAsync(id) as OkObjectResult;
            var responseObject = response.Value as LocationDto;

            Assert.AreEqual(200, response.StatusCode);
            Assert.AreEqual(expectedResult.Id, responseObject.Id);
            Assert.AreEqual(expectedResult.Floor, responseObject.Floor);
            Assert.AreEqual(expectedResult.Cube, responseObject.Cube);
        }

        [Test]
        [TestCase(430)]
        public async Task GetByIdAsync_Return404NotFound(int id)
        {
            var response = await _locationsController.GetByIdAsync(id) as StatusCodeResult;
            Assert.AreEqual(404, response.StatusCode);
        }

        [Test]
        [TestCase(-5)]
        public async Task GetByIdAsync_InValidParameter_Return400BadRequest(int id)
        {
            var response = await _locationsController.GetByIdAsync(id) as StatusCodeResult;

            Assert.AreEqual(400, response.StatusCode);
        }

        [Test]
        public async Task AddAsync_Return201Created()
        {
            var insertedLocation = new CreateLocationDto
            {
                Floor = 6,
                Cube = "FlyTeam"
            };

            var response = await _locationsController.AddAsync(insertedLocation) as CreatedAtActionResult;

            Assert.AreEqual(201, response.StatusCode);

            Assert.AreEqual(_locations.Last().Floor, insertedLocation.Floor);
            Assert.AreEqual(_locations.Last().Cube, insertedLocation.Cube);
        }

        [Test]
        public async Task AddAsync_Return400BadRequest()
        {
            var insertedLocation = new CreateLocationDto
            {
                Cube = "FlyTeam"
            };

            _locationsController.ModelState.AddModelError("Floor", "Floor is invalid");

            var response = await _locationsController.AddAsync(insertedLocation) as BadRequestResult;

            Assert.AreEqual(400, response.StatusCode);

        }

        [Test]
        [TestCase("4")]
        public async Task UpdateAsync_Return204NoContent(int id)
        {

            var updatedLocation = new UpdateLocationDto
            {
                Id = 4,
                Floor = 0,
                Cube = "Test"
            };

            var response = await _locationsController.UpdateAsync(id, updatedLocation) as NoContentResult;
            var inDatabase = _locations.SingleOrDefault(e => e.Id == id);

            Assert.AreEqual(204, response.StatusCode);
            Assert.AreEqual(updatedLocation.Floor, inDatabase.Floor);
            Assert.AreEqual(updatedLocation.Cube, inDatabase.Cube);
        }

        [Test]
        [TestCase(4)]
        public async Task UpdateAsync_InvalidModelState_Return400BadRequest(int id)
        {

            var updatedLocation = new UpdateLocationDto
            {
                Id = 4,
                Floor = 0,
                Cube = "Test"
            };

            _locationsController.ModelState.AddModelError("Phone", "Phone is required");
            var response = await _locationsController.UpdateAsync(id, updatedLocation) as BadRequestResult;

            Assert.AreEqual(400, response.StatusCode);
        }

        [Test]
        [TestCase(-1)]
        public async Task UpdateAsync_InvalidId_Return400BadRequest(int id)
        {

            var updatedLocation = new UpdateLocationDto
            {
                Id = 4,
                Floor = 0,
                Cube = "Test"
            };

            var response = await _locationsController.UpdateAsync(id, updatedLocation) as BadRequestResult;

            Assert.AreEqual(400, response.StatusCode);
        }

        [Test]
        [TestCase(3)]
        public async Task UpdateAsync_MismatchId_Return400BadRequest(int id)
        {

            var updatedLocation = new UpdateLocationDto
            {
                Id = 4,
                Floor = 0,
                Cube = "Test"
            };

            var response = await _locationsController.UpdateAsync(id, updatedLocation) as BadRequestResult;

            Assert.AreEqual(400, response.StatusCode);
        }

        [Test]
        [TestCase(350)]
        public async Task UpdateAsync_NotFound_Return404NotFound(int id)
        {

            var updatedLocation = new UpdateLocationDto
            {
                Id = 350,
                Floor = 0,
                Cube = "Test"
            };

            var response = await _locationsController.UpdateAsync(id, updatedLocation) as NotFoundResult;

            Assert.AreEqual(404, response.StatusCode);
        }

        [Test]
        [TestCase(4)]
        public async Task DeleteAsync_Return204NoContent(int id)
        {

            var response = await _locationsController.DeleteAsync(id) as NoContentResult;
            var inDatabase = _locations.FirstOrDefault(e => e.Id == id);

            Assert.AreEqual(204, response.StatusCode);
            Assert.AreEqual(true, inDatabase.IsDelete);
        }

        [Test]
        [TestCase(-1)]
        public async Task DeleteAsync_InvalidId_Return400BadRequest(int id)
        {

            var response = await _locationsController.DeleteAsync(id) as BadRequestResult;

            Assert.AreEqual(400, response.StatusCode);
        }

        [Test]
        [TestCase(1000)]
        public async Task DeleteAsync_InvalidId_Return404NotFound(int id)
        {

            var response = await _locationsController.DeleteAsync(id) as NotFoundResult;

            Assert.AreEqual(404, response.StatusCode);
        }

        [Test]
        [TestCase(4)]
        public async Task HardDeleteAsync_Return204NoContent(int id)
        {

            var response = await _locationsController.HardDeleteAsync(id) as NoContentResult;
            bool locationInDatabase = _locations.Any(e => e.Id == id);
            bool propertiesInDatabase = _lostProperties.Any(e => e.LocationId == id);

            Assert.AreEqual(204, response.StatusCode);
            Assert.True(!locationInDatabase);
            Assert.True(!propertiesInDatabase);
        }

        [Test]
        [TestCase(-5)]
        public async Task HardDeleteAsync_InvalidId_Return400BadRequest(int id)
        {

            var response = await _locationsController.HardDeleteAsync(id) as BadRequestResult;

            Assert.AreEqual(400, response.StatusCode);
        }

        [Test]
        [TestCase(1000)]
        public async Task HardDeleteAsync_NotFound_Return204NoContent(int id)
        {

            var response = await _locationsController.HardDeleteAsync(id) as NoContentResult;

            Assert.AreEqual(204, response.StatusCode);
        }

        #endregion

        #region TearDown
        [TearDown]
        public void DisposeTest()
        {
            _locationService = null;
            _mapper = null;
            _locationsController = null;
            _lostProperties = null;
            _locations = null;
        }
        #endregion

        #region TestFixture TearDown
        [OneTimeTearDown]
        public void DisposeAllObjects()
        {
            _lostProperties = null;
            _locations = null;
        }
        #endregion
    }
}
