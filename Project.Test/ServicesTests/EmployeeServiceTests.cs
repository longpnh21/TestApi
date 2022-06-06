using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Project.Core.Common;
using Project.Core.Entities;
using Project.Infrastructure.Common;
using Project.Infrastructure.Repositories;
using Project.Services;
using Project.Tests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Project.Tests.ServicesTests
{
    [TestFixture]
    public class EmployeeServiceTests
    {
        #region Variables
        private IEmployeeService _employeeService;
        private ILostPropertyRepository _lostPropertyRepository;
        private IUnitOfWork _unitOfWork;
        private List<Employee> _employees;
        private List<LostProperty> _lostProperties;
        private IEmployeeRepository _employeeRepository;
        private IApplicationDbContext _applicationDbContext;
        #endregion

        #region Test fixture setup
        [OneTimeSetUp]
        public void SetUp()
        {
            _employees = DataInitializer.GetAllEmployees();
            _lostProperties = DataInitializer.GetAllLostProperties();
        }
        #endregion

        #region Setup
        [SetUp]
        public void ReInitializeTest()
        {
            _employees = DataInitializer.GetAllEmployees();
            _lostProperties = DataInitializer.GetAllLostProperties();
            _applicationDbContext = new Mock<IApplicationDbContext>().Object;
            _employeeRepository = SetUpEmployeeRepository();
            _lostPropertyRepository = SetUpLostPropertyRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.EmployeeRepository).Returns(_employeeRepository);
            unitOfWork.SetupGet(x => x.LostPropertyRepository).Returns(_lostPropertyRepository);
            _unitOfWork = unitOfWork.Object;
            _employeeService = new EmployeeService(unitOfWork.Object);
        }
        #endregion

        #region Unit Tests
        [Test]
        [TestCase(1, 2)]
        public async Task GetAllAsync_ReturnCollection(int pageIndex, int pageSize)
        {
            var employees = await _employeeService.GetAsync(pageIndex, pageSize);
            CollectionAssert.AreEqual(employees.Result, _employees.Skip((pageIndex - 1) * pageSize).Take(pageSize));
        }

        [Test]
        [TestCase("7C39DD64-4643-B552-1213-1243D9F5D643")]
        public async Task GetByIdAsync_ReturnEmployee(string id)
        {
            var employee = await _employeeService.GetByIdAsync(id);
            Assert.AreEqual(_employees[0], employee);
        }

        [Test]
        [TestCase("not available id")]
        public async Task GetByIdAsync_ReturnNull(string id)
        {
            var employee = await _employeeService.GetByIdAsync(id);
            Assert.Null(employee);
        }

        [Test]
        public async Task AddAsync()
        {
            var insertedEmployee = new Employee
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "Create",
                LastName = "Test",
                Phone = "1-142-253-5067",
                Email = "ut.molestie@aol.edu",
                DateOfBirth = DateTime.Parse("1970-11-18")
            };

            await _employeeService.AddAsync(insertedEmployee);
            Assert.AreEqual(_employees.Last(), insertedEmployee);
        }

        [Test]
        public async Task UpdateAsync()
        {
            var employee = new Employee
            {
                Id = "EA2E7616-1428-2E6E-9858-136E89E456C9",
                FirstName = "Galvin",
                LastName = "Carlson",
                Phone = "120-2165",
                Email = "convallis.est@protonmail.couk",
                DateOfBirth = DateTime.Parse("1934-08-25")
            };

            var updatedEmployee = new Employee
            {
                Id = "EA2E7616-1428-2E6E-9858-136E89E456C9",
                FirstName = "Update",
                LastName = "Test",
                Phone = "120-2165",
                Email = "convallis.est@protonmail.couk",
                DateOfBirth = DateTime.Parse("1934-08-25")
            };

            await _employeeService.UpdateAsync(updatedEmployee);
            var changedEmployee = _employees.FirstOrDefault(x => x.Id == employee.Id);
            Assert.True(changedEmployee.FirstName == "Update"
                && changedEmployee.LastName == "Test");
        }

        [Test]
        [TestCase("EA2E7616-1428-2E6E-9858-136E89E456C9")]
        public async Task DeleteAsync(string id)
        {

            await _employeeService.DeleteAsync(id);
            var deletedEmployee = _employees.FirstOrDefault(x => x.Id == id);
            Assert.True(deletedEmployee.IsDelete);
        }

        [Test]
        [TestCase("7C39DD64-4643-B552-1213-1243D9F5D643")]
        public async Task HardDeleteAsync(string id)
        {

            var preDeletedEmployee = _employees.FirstOrDefault(e => e.Id == id);
            await _employeeService.HardDeleteAsync(id);
            var deletedEmployee = _employees.FirstOrDefault(x => x.Id == id);
            Assert.True((preDeletedEmployee is not null)
                && (deletedEmployee is null));
        }
        #endregion

        #region Private member methods
        private ILostPropertyRepository SetUpLostPropertyRepository()
        {
            var mockRepo = new Mock<LostPropertyRepository>(MockBehavior.Default, _applicationDbContext);

            mockRepo.Setup(x => x.GetAllAsync(
                It.IsAny<Expression<Func<LostProperty, bool>>>(),
                It.IsAny<Func<IQueryable<LostProperty>, IOrderedQueryable<LostProperty>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(
                    (Expression<Func<LostProperty, bool>> filter,
                    Func<IQueryable<LostProperty>, IOrderedQueryable<LostProperty>> orderBy,
                    string includeProperties,
                    bool isDelete
                    ) =>
                    {
                        var query = _lostProperties.AsQueryable();

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
                            ? new PaginatedList<LostProperty>(orderBy(query), query.Count())
                            : new PaginatedList<LostProperty>(query, query.Count());
                    }
                );

            mockRepo.Setup(x => x.HardDeleteAsync(It.IsAny<IEnumerable<LostProperty>>()))
                .Callback(new Action<IEnumerable<LostProperty>>(propertiesToDelete =>
                {
                    var temp = new List<LostProperty>();
                    temp = propertiesToDelete.ToList();
                    foreach (var property in temp)
                    {
                        _lostProperties.Remove(property);
                    }
                }));

            return mockRepo.Object;
        }
        private IEmployeeRepository SetUpEmployeeRepository()
        {
            var mockRepo = new Mock<EmployeeRepository>(MockBehavior.Default, _applicationDbContext);

            mockRepo.Setup(x => x.GetWithPaginationAsync(
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

            mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Func<string, Employee>(id => _employees.FirstOrDefault(e => e.Id == id)));

            mockRepo.Setup(p => p.InsertAsync(It.IsAny<Employee>()))
                .Callback(new Action<Employee>(newEmployee =>
                {
                    string employeeId = Guid.NewGuid().ToString();
                    newEmployee.Id = employeeId;
                    _employees.Add(newEmployee);
                }));

            mockRepo.Setup(x => x.UpdateAsync(It.IsAny<Employee>()))
                .Callback(new Action<Employee>(emp =>
                   {
                       int oldEmployee = _employees.FindIndex(e => e.Id == emp.Id);
                       _employees[oldEmployee] = emp;
                   }));

            mockRepo.Setup(x => x.DeleteAsync(It.IsAny<object>()))
                .Callback((object empId) =>
                {
                    string id = empId.ToString();
                    var employeeToRemove = _employees.FirstOrDefault(e => e.Id == id);
                    if (employeeToRemove is not null)
                    {
                        employeeToRemove.IsDelete = true;
                    }
                });

            mockRepo.Setup(x => x.DeleteAsync(It.IsAny<Employee>()))
                .Callback(new Action<Employee>(emp =>
                {
                    var employeeToRemove = _employees.FirstOrDefault(e => e.Id == emp.Id);
                    if (employeeToRemove is not null)
                    {
                        employeeToRemove.IsDelete = true;
                    }
                }));

            mockRepo.Setup(x => x.HardDeleteAsync(It.IsAny<object>()))
                .Callback((object empId) =>
                {
                    string id = empId.ToString();
                    var employeeToRemove = _employees.FirstOrDefault(e => e.Id == id);
                    if (employeeToRemove is not null)
                    {
                        _employees.Remove(employeeToRemove);
                    }
                });

            mockRepo.Setup(x => x.HardDeleteAsync(It.IsAny<Employee>()))
                .Callback(new Action<Employee>(emp =>
                {
                    var employeeToRemove = _employees.FirstOrDefault(e => e.Id == emp.Id);
                    if (employeeToRemove is not null)
                    {
                        _employees.Remove(employeeToRemove);
                    }
                }));

            return mockRepo.Object;
        }
        #endregion

        #region TearDown
        [TearDown]
        public void DisposeTest()
        {
            _employeeService = null;
            _unitOfWork = null;
            _employeeRepository = null;
            if (_applicationDbContext is not null)
            {
                _applicationDbContext.Dispose();
            }
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
