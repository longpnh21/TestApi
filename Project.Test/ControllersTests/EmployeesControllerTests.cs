using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Project.Api.Controllers;
using Project.Application.AutoMapper;
using Project.Application.Dtos.Employee;
using Project.Core.Common;
using Project.Core.Entities;
using Project.Services;
using Project.Tests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Project.Tests.ControllersTests
{
    [TestFixture]
    public class EmployeesControllerTest
    {
        #region Variables
        private IEmployeeService _employeeService;
        private List<Employee> _employees;
        private List<LostProperty> _lostProperties;
        private IMapper _mapper;
        private EmployeesController _employeesController;
        #endregion

        #region Test fixture setup
        [SetUp]
        public void Setup()
        {
            _employees = DataInitializer.GetAllEmployees();
            _lostProperties = DataInitializer.GetAllLostProperties();
            _employeeService = SetUpEmployeeService();
            _mapper = SetupAutoMapper();
            _employeesController = new EmployeesController(_employeeService, _mapper);
        }

        #endregion

        #region Setup
        [OneTimeSetUp]
        public void ReInitializeTest()
        {
            _employees = DataInitializer.GetAllEmployees();
            _lostProperties = DataInitializer.GetAllLostProperties();
        }
        #endregion

        #region Private member methods

        private IEmployeeService SetUpEmployeeService()
        {
            var mockService = new Mock<IEmployeeService>();

            mockService.Setup(x => x.AddAsync(It.IsAny<Employee>()))
                .Callback(new Action<Employee>(newEmployee =>
            {
                string employeeId = Guid.NewGuid().ToString();
                newEmployee.Id = employeeId;
                _employees.Add(newEmployee);
            }));
            mockService.Setup(x => x.UpdateAsync(It.IsAny<Employee>()))
                .Callback(new Action<Employee>(updatedEmployee =>
                {
                    int oldEmployee = _employees.FindIndex(e => e.Id == updatedEmployee.Id);
                    _employees[oldEmployee] = updatedEmployee;
                }));
            mockService.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Func<string, Employee>(id => _employees.FirstOrDefault(e => e.Id == id)));

            mockService.Setup(x => x.DeleteAsync(It.IsAny<string>()))
                .Callback((string id) =>
                {
                    var employeeToRemove = _employees.FirstOrDefault(e => e.Id == id);
                    if (employeeToRemove is null)
                    {
                        throw new ArgumentNullException();
                    }
                    employeeToRemove.IsDelete = true;
                });
            mockService.Setup(x => x.HardDeleteAsync(It.IsAny<string>()))
                .Callback((string id) =>
                {
                    var employeeToRemove = _employees.FirstOrDefault(e => e.Id == id);
                    if (employeeToRemove is not null)
                    {
                        var employeeProperties = _lostProperties.Where(p => id == p.EmployeeId).ToList();
                        var temp = new List<LostProperty>();
                        temp = employeeProperties;
                        foreach (var property in temp)
                        {
                            _lostProperties.Remove(property);
                        }

                        _employees.Remove(employeeToRemove);
                    }
                });

            mockService.Setup(x => x.GetAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Employee, bool>>>(),
                It.IsAny<Func<IQueryable<Employee>, IOrderedQueryable<Employee>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(
                    (int pageIndex,
                    int pageSize,
                    Expression<Func<Employee, bool>> filter,
                    Func<IQueryable<Employee>, IOrderedQueryable<Employee>> orderBy,
                    string includeProperties,
                    bool isDelete
                    ) =>
                    {
                        var query = _employees.AsQueryable();

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
                            ? new PaginatedList<Employee>(orderBy(query), query.Count(), pageIndex, pageSize)
                            : new PaginatedList<Employee>(query, query.Count(), pageIndex, pageSize);
                    }
                );

            return mockService.Object;
        }

        private IMapper SetupAutoMapper() => new MapperConfiguration(cfg => cfg.AddProfile<ApplicationProfile>()).CreateMapper();
        #endregion

        #region Unit tests
        [Test]
        [TestCase(1, 3)]
        public async Task GetAsync_Return200Ok(int pageIndex = 1, int pageSize = 10)
        {

            var response = await _employeesController.GetAsync(pageIndex, pageSize) as OkObjectResult;
            var responseObject = response.Value as PaginatedList<EmployeeDto>;
            var expected = _employees.Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(e => _mapper.Map<EmployeeDto>(e)).ToList();
            Assert.AreEqual(200, response.StatusCode);
            Assert.AreEqual(expected.Count, responseObject.Result.Count);
        }

        [Test]
        [TestCase("7C39DD64-4643-B552-1213-1243D9F5D643")]
        public async Task GetByIdAsync_Return200Ok(string id)
        {
            var expectedResult = _mapper.Map<EmployeeDto>(_employees.FirstOrDefault(e => e.Id == id));

            var response = await _employeesController.GetByIdAsync(id) as OkObjectResult;
            var responseObject = response.Value as EmployeeDto;

            Assert.AreEqual(200, response.StatusCode);
            Assert.AreEqual(expectedResult.Id, responseObject.Id);
            Assert.AreEqual(expectedResult.FirstName, responseObject.FirstName);
            Assert.AreEqual(expectedResult.LastName, responseObject.LastName);
            Assert.AreEqual(expectedResult.Phone, responseObject.Phone);
            Assert.AreEqual(expectedResult.Email, responseObject.Email);
            Assert.AreEqual(expectedResult.DateOfBirth, responseObject.DateOfBirth);
        }

        [Test]
        [TestCase("7C395D64-4643-B552-1213-1243D9F5D643")]
        public async Task GetByIdAsync_Return404NotFound(string id)
        {
            var response = await _employeesController.GetByIdAsync(id) as StatusCodeResult;
            Assert.AreEqual(404, response.StatusCode);
        }

        [Test]
        [TestCase("")]
        public async Task GetByIdAsync_InValidParameter_Return400BadRequest(string id)
        {
            var response = await _employeesController.GetByIdAsync(id) as StatusCodeResult;

            Assert.AreEqual(400, response.StatusCode);
        }

        [Test]
        public async Task AddAsync_Return201Created()
        {
            var insertedEmployee = new CreateEmployeeDto
            {
                FirstName = "Create",
                LastName = "Test",
                Phone = "1-142-253-5067",
                Email = "ut.molestie@aol.edu",
                DateOfBirth = DateTime.Parse("1970-11-18")
            };

            var response = await _employeesController.AddAsync(insertedEmployee) as CreatedAtActionResult;

            Assert.AreEqual(201, response.StatusCode);
            Assert.AreEqual(_employees.Last().FirstName, insertedEmployee.FirstName);
            Assert.AreEqual(_employees.Last().LastName, insertedEmployee.LastName);
            Assert.AreEqual(_employees.Last().Phone, insertedEmployee.Phone);
            Assert.AreEqual(_employees.Last().Email, insertedEmployee.Email);
            Assert.AreEqual(_employees.Last().DateOfBirth, insertedEmployee.DateOfBirth);
        }

        [Test]
        public async Task AddAsync_Return400BadRequest()
        {
            var insertedEmployee = new CreateEmployeeDto
            {
                LastName = "Test",
                Phone = "1-142-253-5067",
                Email = "ut.molestie@aol.edu",
                DateOfBirth = DateTime.Parse("1970-11-18")
            };

            _employeesController.ModelState.AddModelError("FirstName", "FirstName is required");

            var response = await _employeesController.AddAsync(insertedEmployee) as BadRequestResult;

            Assert.AreEqual(400, response.StatusCode);

        }

        [Test]
        [TestCase("EA2E7616-1428-2E6E-9858-136E89E456C9")]
        public async Task UpdateAsync_Return204NoContent(string id)
        {

            var updatedEmployee = new UpdateEmployeeDto
            {
                Id = "EA2E7616-1428-2E6E-9858-136E89E456C9",
                FirstName = "Update",
                LastName = "Test",
                Phone = "120-2165",
                Email = "convallis.est@protonmail.couk",
                DateOfBirth = DateTime.Parse("1934-08-25")
            };

            var response = await _employeesController.UpdateAsync(id, updatedEmployee) as NoContentResult;
            var inDatabase = _employees.SingleOrDefault(e => e.Id == id);

            Assert.AreEqual(204, response.StatusCode);
            Assert.AreEqual(inDatabase.FirstName, updatedEmployee.FirstName);
            Assert.AreEqual(inDatabase.LastName, updatedEmployee.LastName);
            Assert.AreEqual(inDatabase.Phone, updatedEmployee.Phone);
            Assert.AreEqual(inDatabase.Email, updatedEmployee.Email);
            Assert.AreEqual(inDatabase.DateOfBirth, updatedEmployee.DateOfBirth);
        }

        [Test]
        [TestCase("EA2E7616-1428-2E6E-9858-136E89E456C9")]
        public async Task UpdateAsync_InvalidModelState_Return400BadRequest(string id)
        {

            var updatedEmployee = new UpdateEmployeeDto
            {
                Id = "EA2E7616-1428-2E6E-9858-136E89E456C9",
                FirstName = "Update",
                LastName = "Test",
                Email = "convallis.est@protonmail.couk",
                DateOfBirth = DateTime.Parse("1934-08-25")
            };

            _employeesController.ModelState.AddModelError("Phone", "Phone is required");
            var response = await _employeesController.UpdateAsync(id, updatedEmployee) as BadRequestResult;

            Assert.AreEqual(400, response.StatusCode);
        }

        [Test]
        [TestCase("")]
        public async Task UpdateAsync_InvalidId_Return400BadRequest(string id)
        {

            var updatedEmployee = new UpdateEmployeeDto
            {
                Id = "EA2E7616-1428-2E6E-9858-136E89E456C9",
                FirstName = "Update",
                LastName = "Test",
                Email = "convallis.est@protonmail.couk",
                DateOfBirth = DateTime.Parse("1934-08-25")
            };

            var response = await _employeesController.UpdateAsync(id, updatedEmployee) as BadRequestResult;

            Assert.AreEqual(400, response.StatusCode);
        }

        [Test]
        [TestCase("AA2E7616-1428-2E6E-9858-136E89E456C9")]
        public async Task UpdateAsync_MismatchId_Return400BadRequest(string id)
        {

            var updatedEmployee = new UpdateEmployeeDto
            {
                Id = "EA2E7616-1428-2E6E-9858-136E89E456C9",
                FirstName = "Update",
                LastName = "Test",
                Email = "convallis.est@protonmail.couk",
                DateOfBirth = DateTime.Parse("1934-08-25")
            };

            var response = await _employeesController.UpdateAsync(id, updatedEmployee) as BadRequestResult;

            Assert.AreEqual(400, response.StatusCode);
        }

        [Test]
        [TestCase("AA2E7616-1428-2E6E-9858-136E89E456C9")]
        public async Task UpdateAsync_NotFound_Return404NotFound(string id)
        {

            var updatedEmployee = new UpdateEmployeeDto
            {
                Id = "AA2E7616-1428-2E6E-9858-136E89E456C9",
                FirstName = "Update",
                LastName = "Test",
                Email = "convallis.est@protonmail.couk",
                DateOfBirth = DateTime.Parse("1934-08-25")
            };

            var response = await _employeesController.UpdateAsync(id, updatedEmployee) as NotFoundResult;

            Assert.AreEqual(404, response.StatusCode);
        }

        [Test]
        [TestCase("7C39DD64-4643-B552-1213-1243D9F5D643")]
        public async Task DeleteAsync_Return204NoContent(string id)
        {

            var response = await _employeesController.DeleteAsync(id) as NoContentResult;
            var inDatabase = _employees.FirstOrDefault(e => e.Id == id);

            Assert.AreEqual(204, response.StatusCode);
            Assert.AreEqual(true, inDatabase.IsDelete);
        }

        [Test]
        [TestCase("")]
        public async Task DeleteAsync_InvalidId_Return400BadRequest(string id)
        {

            var response = await _employeesController.DeleteAsync(id) as BadRequestResult;

            Assert.AreEqual(400, response.StatusCode);
        }

        [Test]
        [TestCase("AA2E7616-1428-2E6E-9858-136E89E456C9")]
        public async Task DeleteAsync_InvalidId_Return404NotFound(string id)
        {

            var response = await _employeesController.DeleteAsync(id) as NotFoundResult;

            Assert.AreEqual(404, response.StatusCode);
        }

        [Test]
        [TestCase("EA2E7616-1428-2E6E-9858-136E89E456C9")]
        public async Task HardDeleteAsync_Return204NoContent(string id)
        {

            var response = await _employeesController.HardDeleteAsync(id) as NoContentResult;
            bool employeeInDatabase = _employees.Any(e => e.Id == id);
            bool propertiesInDatabase = _lostProperties.Any(e => e.EmployeeId == id);

            Assert.AreEqual(204, response.StatusCode);
            Assert.True(!employeeInDatabase);
            Assert.True(!propertiesInDatabase);
        }

        [Test]
        [TestCase("")]
        public async Task HardDeleteAsync_InvalidId_Return400BadRequest(string id)
        {

            var response = await _employeesController.HardDeleteAsync(id) as BadRequestResult;

            Assert.AreEqual(400, response.StatusCode);
        }

        [Test]
        [TestCase("AA2E7616-1428-2E6E-9858-136E89E456C9")]
        public async Task HardDeleteAsync_NotFound_Return204NoContent(string id)
        {

            var response = await _employeesController.HardDeleteAsync(id) as NoContentResult;

            Assert.AreEqual(204, response.StatusCode);
        }

        #endregion

        #region TearDown
        [TearDown]
        public void DisposeTest()
        {
            _employeeService = null;
            _mapper = null;
            _employeesController = null;
            _lostProperties = null;
            _employees = null;
        }
        #endregion

        #region TestFixture TearDown
        [OneTimeTearDown]
        public void DisposeAllObjects()
        {
            _lostProperties = null;
            _employees = null;
        }
        #endregion
    }
}