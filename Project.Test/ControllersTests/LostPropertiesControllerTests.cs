using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Project.Api.Controllers;
using Project.Application.AutoMapper;
using Project.Application.Dtos.LostProperty;
using Project.Core.Common.Enum;
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
    public class LostPropertiesControllerTests
    {
        #region Variables
        private ILostPropertyService _lostPropertyService;
        private List<LostProperty> _lostProperties;
        private List<Employee> _employees;
        private List<Location> _locations;
        private IMapper _mapper;
        private LostPropertiesController _lostPropertiesController;
        #endregion

        #region Test fixture setup
        [SetUp]
        public void Setup()
        {
            _lostProperties = DataInitializer.GetAllLostProperties();
            _lostPropertyService = SetUpLostPropertyService();
            _mapper = SetupAutoMapper();
            _lostPropertiesController = new LostPropertiesController(_lostPropertyService, _mapper);
        }

        #endregion

        #region Setup
        [OneTimeSetUp]
        public void ReInitializeTest()
        {
            _lostProperties = DataInitializer.GetAllLostProperties();
            _employees = DataInitializer.GetAllEmployees();
            _locations = DataInitializer.GetAllLocations();
        }
        #endregion

        #region Private member methods

        private ILostPropertyService SetUpLostPropertyService()
        {
            var mockService = new Mock<ILostPropertyService>();

            mockService.Setup(x => x.AddAsync(It.IsAny<LostProperty>()))
                .Callback(new Action<LostProperty>(newLostProperty =>
                {
                    int maxId = _lostProperties.Max(e => e.Id);
                    newLostProperty.Id = maxId++;
                    if (string.IsNullOrWhiteSpace(newLostProperty.EmployeeId))
                    {
                        if (_employees.Any(e => e.Id == newLostProperty.EmployeeId))
                        {
                            throw new ArgumentNullException();
                        }
                    }
                    if (newLostProperty.LocationId != null)
                    {
                        if (_locations.Any(e => e.Id == newLostProperty.LocationId))
                        {
                            throw new ArgumentNullException();
                        }
                    }
                    _lostProperties.Add(newLostProperty);
                }));
            mockService.Setup(x => x.UpdateAsync(It.IsAny<LostProperty>()))
                .Callback(new Action<LostProperty>(updatedLostProperty =>
                {
                    int oldLostProperty = _lostProperties.FindIndex(e => e.Id == updatedLostProperty.Id);
                    _lostProperties[oldLostProperty] = updatedLostProperty;
                }));
            mockService.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Func<int, LostProperty>(id => _lostProperties.FirstOrDefault(e => e.Id == id)));

            mockService.Setup(x => x.DeleteAsync(It.IsAny<int>()))
                .Callback((int id) =>
                {
                    var lostPropertyToRemove = _lostProperties.FirstOrDefault(e => e.Id == id);
                    if (lostPropertyToRemove is null)
                    {
                        throw new ArgumentNullException();
                    }
                    lostPropertyToRemove.IsDelete = true;
                });
            mockService.Setup(x => x.HardDeleteAsync(It.IsAny<int>()))
                .Callback((int id) =>
                {
                    var lostPropertyToRemove = _lostProperties.FirstOrDefault(e => e.Id == id);
                    if (lostPropertyToRemove is not null)
                    {
                        var lostPropertyProperties = _lostProperties.Where(p => id == p.Id).ToList();
                        var temp = new List<LostProperty>();
                        temp = lostPropertyProperties;
                        foreach (var property in temp)
                        {
                            _lostProperties.Remove(property);
                        }

                        _lostProperties.Remove(lostPropertyToRemove);
                    }
                });

            mockService.Setup(x => x.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<LostProperty, bool>>>(),
                It.IsAny<Func<IQueryable<LostProperty>, IOrderedQueryable<LostProperty>>>(),
                It.IsAny<string>()))
                .ReturnsAsync(
                    (int pageIndex,
                    int pageSize,
                    Expression<Func<LostProperty, bool>> filter,
                    Func<IQueryable<LostProperty>, IOrderedQueryable<LostProperty>> orderBy,
                    string includeProperties
                    ) =>
                    {
                        var query = _lostProperties.AsQueryable();

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
                            : (IEnumerable<LostProperty>)query
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

            var response = await _lostPropertiesController.GetAsync(pageIndex, pageSize) as OkObjectResult;
            var responseObject = response.Value as IEnumerable<LostPropertyDto>;
            var expected = _lostProperties.Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(e => _mapper.Map<LostPropertyDto>(e)).ToList();
            Assert.AreEqual(200, response.StatusCode);
            Assert.AreEqual(expected.Count, responseObject.ToList().Count);
        }

        [Test]
        [TestCase(2)]
        public async Task GetByIdAsync_Return200Ok(int id)
        {
            var expectedResult = _mapper.Map<LostPropertyDto>(_lostProperties.FirstOrDefault(e => e.Id == id));

            var response = await _lostPropertiesController.GetByIdAsync(id) as OkObjectResult;
            var responseObject = response.Value as LostPropertyDto;

            Assert.AreEqual(200, response.StatusCode);
            Assert.AreEqual(expectedResult.Id, responseObject.Id);
            Assert.AreEqual(expectedResult.Name, responseObject.Name);
            Assert.AreEqual(expectedResult.Description, responseObject.Description);
            Assert.AreEqual(expectedResult.Status, responseObject.Status);
            Assert.AreEqual(expectedResult.LocationId, responseObject.LocationId);
            Assert.AreEqual(expectedResult.FoundTime, responseObject.FoundTime);
            Assert.AreEqual(expectedResult.EmployeeId, responseObject.EmployeeId);
            Assert.AreEqual(expectedResult.CreationTime, responseObject.CreationTime);
            Assert.AreEqual(expectedResult.LastModifiedTime, responseObject.LastModifiedTime);
        }

        [Test]
        [TestCase(430)]
        public async Task GetByIdAsync_Return404NotFound(int id)
        {
            var response = await _lostPropertiesController.GetByIdAsync(id) as StatusCodeResult;
            Assert.AreEqual(404, response.StatusCode);
        }

        [Test]
        [TestCase(-5)]
        public async Task GetByIdAsync_InValidParameter_Return400BadRequest(int id)
        {
            var response = await _lostPropertiesController.GetByIdAsync(id) as StatusCodeResult;

            Assert.AreEqual(400, response.StatusCode);
        }

        [Test]
        public async Task AddAsync_Return201Created()
        {
            var insertedLostProperty = new CreateLostPropertyDto
            {
                Name = "KMS employee mug",
                Description = "semper cursus. Integer mollis. Integer tincidunt aliquam",
                Status = PropertyStatus.Lost,
                FoundTime = DateTime.Parse("2023-03-01"),
                EmployeeId = null
            };

            var response = await _lostPropertiesController.AddAsync(insertedLostProperty) as CreatedAtActionResult;

            Assert.AreEqual(201, response.StatusCode);
            Assert.AreEqual(insertedLostProperty.Name, _lostProperties.Last().Name);
            Assert.AreEqual(insertedLostProperty.Description, _lostProperties.Last().Description);
            Assert.AreEqual(insertedLostProperty.Status, _lostProperties.Last().Status);
            Assert.AreEqual(insertedLostProperty.LocationId, _lostProperties.Last().LocationId);
            Assert.AreEqual(insertedLostProperty.FoundTime, _lostProperties.Last().FoundTime);
            Assert.AreEqual(insertedLostProperty.EmployeeId, _lostProperties.Last().EmployeeId);
        }

        [Test]
        public async Task AddAsync_Return400BadRequest()
        {
            var insertedLostProperty = new CreateLostPropertyDto
            {
                Name = "KMS employee mug",
                Description = "semper cursus. Integer mollis. Integer tincidunt aliquam",
                Status = PropertyStatus.Lost,
                FoundTime = DateTime.Parse("2023-03-01"),
                EmployeeId = null
            };

            _lostPropertiesController.ModelState.AddModelError("Floor", "Floor is invalid");

            var response = await _lostPropertiesController.AddAsync(insertedLostProperty) as BadRequestResult;

            Assert.AreEqual(400, response.StatusCode);

        }

        [Test]
        [TestCase("4")]
        public async Task UpdateAsync_Return204NoContent(int id)
        {

            var updatedLostProperty = new UpdateLostPropertyDto
            {
                Id = 4,
                Name = "KMS employee mug",
                Description = "semper cursus. Integer mollis. Integer tincidunt aliquam",
                Status = PropertyStatus.Lost,
                FoundTime = DateTime.Parse("2023-03-01"),
                EmployeeId = null
            };

            var response = await _lostPropertiesController.UpdateAsync(id, updatedLostProperty) as NoContentResult;
            var inDatabase = _lostProperties.SingleOrDefault(e => e.Id == id);

            Assert.AreEqual(204, response.StatusCode);
            Assert.AreEqual(updatedLostProperty.Name, inDatabase.Name);
            Assert.AreEqual(updatedLostProperty.Description, inDatabase.Description);
            Assert.AreEqual(updatedLostProperty.Status, inDatabase.Status);
            Assert.AreEqual(updatedLostProperty.LocationId, inDatabase.LocationId);
            Assert.AreEqual(updatedLostProperty.FoundTime, inDatabase.FoundTime);
            Assert.AreEqual(updatedLostProperty.EmployeeId, inDatabase.EmployeeId);
        }

        [Test]
        [TestCase(4)]
        public async Task UpdateAsync_InvalidModelState_Return400BadRequest(int id)
        {

            var updatedLostProperty = new UpdateLostPropertyDto
            {
                Id = 4,
                Name = "KMS employee mug",
                Description = "semper cursus. Integer mollis. Integer tincidunt aliquam",
                Status = PropertyStatus.Lost,
                FoundTime = DateTime.Parse("2023-03-01"),
                EmployeeId = null
            };

            _lostPropertiesController.ModelState.AddModelError("Phone", "Phone is required");
            var response = await _lostPropertiesController.UpdateAsync(id, updatedLostProperty) as BadRequestResult;

            Assert.AreEqual(400, response.StatusCode);
        }

        [Test]
        [TestCase(-1)]
        public async Task UpdateAsync_InvalidId_Return400BadRequest(int id)
        {

            var updatedLostProperty = new UpdateLostPropertyDto
            {
                Id = 4,
                Name = "KMS employee mug",
                Description = "semper cursus. Integer mollis. Integer tincidunt aliquam",
                Status = PropertyStatus.Lost,
                FoundTime = DateTime.Parse("2023-03-01"),
                EmployeeId = null
            };

            var response = await _lostPropertiesController.UpdateAsync(id, updatedLostProperty) as BadRequestResult;

            Assert.AreEqual(400, response.StatusCode);
        }

        [Test]
        [TestCase(3)]
        public async Task UpdateAsync_MismatchId_Return400BadRequest(int id)
        {

            var updatedLostProperty = new UpdateLostPropertyDto
            {
                Id = 4,
                Name = "KMS employee mug",
                Description = "semper cursus. Integer mollis. Integer tincidunt aliquam",
                Status = PropertyStatus.Lost,
                FoundTime = DateTime.Parse("2023-03-01"),
                EmployeeId = null
            };

            var response = await _lostPropertiesController.UpdateAsync(id, updatedLostProperty) as BadRequestResult;

            Assert.AreEqual(400, response.StatusCode);
        }

        [Test]
        [TestCase(350)]
        public async Task UpdateAsync_NotFound_Return404NotFound(int id)
        {

            var updatedLostProperty = new UpdateLostPropertyDto
            {
                Id = 350,
                Name = "KMS employee mug",
                Description = "semper cursus. Integer mollis. Integer tincidunt aliquam",
                Status = PropertyStatus.Lost,
                FoundTime = DateTime.Parse("2023-03-01"),
                EmployeeId = null
            };

            var response = await _lostPropertiesController.UpdateAsync(id, updatedLostProperty) as NotFoundResult;

            Assert.AreEqual(404, response.StatusCode);
        }

        [Test]
        [TestCase(4)]
        public async Task DeleteAsync_Return204NoContent(int id)
        {

            var response = await _lostPropertiesController.DeleteAsync(id) as NoContentResult;
            var inDatabase = _lostProperties.FirstOrDefault(e => e.Id == id);

            Assert.AreEqual(204, response.StatusCode);
            Assert.AreEqual(true, inDatabase.IsDelete);
        }

        [Test]
        [TestCase(-1)]
        public async Task DeleteAsync_InvalidId_Return400BadRequest(int id)
        {

            var response = await _lostPropertiesController.DeleteAsync(id) as BadRequestResult;

            Assert.AreEqual(400, response.StatusCode);
        }

        [Test]
        [TestCase(1000)]
        public async Task DeleteAsync_InvalidId_Return404NotFound(int id)
        {

            var response = await _lostPropertiesController.DeleteAsync(id) as NotFoundResult;

            Assert.AreEqual(404, response.StatusCode);
        }

        [Test]
        [TestCase(4)]
        public async Task HardDeleteAsync_Return204NoContent(int id)
        {

            var response = await _lostPropertiesController.HardDeleteAsync(id) as NoContentResult;
            bool lostPropertyInDatabase = _lostProperties.Any(e => e.Id == id);

            Assert.AreEqual(204, response.StatusCode);
            Assert.True(!lostPropertyInDatabase);
        }

        [Test]
        [TestCase(-5)]
        public async Task HardDeleteAsync_InvalidId_Return400BadRequest(int id)
        {

            var response = await _lostPropertiesController.HardDeleteAsync(id) as BadRequestResult;

            Assert.AreEqual(400, response.StatusCode);
        }

        [Test]
        [TestCase(1000)]
        public async Task HardDeleteAsync_NotFound_Return204NoContent(int id)
        {

            var response = await _lostPropertiesController.HardDeleteAsync(id) as NoContentResult;

            Assert.AreEqual(204, response.StatusCode);
        }

        #endregion

        #region TearDown
        [TearDown]
        public void DisposeTest()
        {
            _lostPropertyService = null;
            _mapper = null;
            _lostPropertiesController = null;
            _lostProperties = null;
            _employees = null;
            _locations = null;
        }
        #endregion

        #region TestFixture TearDown
        [OneTimeTearDown]
        public void DisposeAllObjects()
        {
            _lostProperties = null;
            _employees = null;
            _locations = null;
        }
        #endregion
    }
}
