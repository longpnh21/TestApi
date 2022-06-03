using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Project.Core.Common.Enum;
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
    public class LostPropertyServiceTests
    {
        #region Variables
        private ILostPropertyService _lostPropertyService;
        private IEmployeeRepository _employeeRepository;
        private IUnitOfWork _unitOfWork;
        private List<LostProperty> _lostProperties;
        private List<Employee> _employees;
        private ILostPropertyRepository _lostPropertyRepository;
        private IApplicationDbContext _applicationDbContext;
        #endregion

        #region Test fixture setup
        [OneTimeSetUp]
        public void SetUp()
        {
            _lostProperties = DataInitializer.GetAllLostProperties();
            _employees = DataInitializer.GetAllEmployees();
        }
        #endregion

        #region Setup
        [SetUp]
        public void ReInitializeTest()
        {
            _lostProperties = DataInitializer.GetAllLostProperties();
            _employees = DataInitializer.GetAllEmployees();
            _applicationDbContext = new Mock<IApplicationDbContext>().Object;
            _lostPropertyRepository = SetUpLostPropertyRepository();
            _employeeRepository = SetUpEmployeeRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.LostPropertyRepository).Returns(_lostPropertyRepository);
            unitOfWork.SetupGet(x => x.EmployeeRepository).Returns(_employeeRepository);
            _unitOfWork = unitOfWork.Object;
            _lostPropertyService = new LostPropertyService(unitOfWork.Object);
        }
        #endregion

        #region Unit Tests
        [Test]
        [TestCase(1, 10)]
        public async Task GetAllAsync_ReturnCollection(int pageIndex, int pageSize)
        {
            var lostProperties = await _lostPropertyService.GetAllAsync(pageIndex, pageSize);
            CollectionAssert.AreEqual(lostProperties, _lostProperties.Skip((pageIndex - 1) * pageSize).Take(pageSize));
        }

        [Test]
        [TestCase(0)]
        public async Task GetByIdAsync_ReturnLostProperty(int id)
        {
            var lostProperty = await _lostPropertyService.GetByIdAsync(id);
            Assert.AreEqual(_lostProperties[id], lostProperty);
        }

        [Test]
        [TestCase(6)]
        public async Task GetByIdAsync_ReturnNull(int id)
        {
            var lostProperty = await _lostPropertyService.GetByIdAsync(id);
            Assert.Null(lostProperty);
        }


        [Test]
        public async Task AddAsync()
        {
            var insertedLostProperty = new LostProperty
            {
                Name = "Asus charger",
                Description = "risus quis diam luctus lobortis. Class",
                Status = PropertyStatus.Lost,
                FoundTime = DateTime.Parse("2021-12-09"),
                EmployeeId = "7C39DD64-4643-B552-1213-1243D9F5D643"
            };

            await _lostPropertyService.AddAsync(insertedLostProperty);
            Assert.AreEqual(_lostProperties.Last(), insertedLostProperty);
        }

        [Test]
        public async Task UpdateAsync()
        {
            var lostProperty = new LostProperty
            {
                Name = "Asus charger",
                Description = "risus quis diam luctus lobortis. Class",
                Status = PropertyStatus.Lost,
                FoundTime = DateTime.Parse("2021-12-09"),
                EmployeeId = "7C39DD64-4643-B552-1213-1243D9F5D643"
            };

            var updatedLostProperty = new LostProperty
            {
                Name = "Asus charger",
                Description = "risus quis diam luctus lobortis. Class",
                Status = PropertyStatus.Lost,
                FoundTime = DateTime.Parse("2021-12-09"),
                EmployeeId = "7C39DD64-4643-B552-1213-1243D9F5D643"
            };

            await _lostPropertyService.UpdateAsync(updatedLostProperty);
            var changedLostProperty = _lostProperties.FirstOrDefault(x => x.Id == lostProperty.Id);

        }

        [Test]
        [TestCase(2)]
        public async Task DeleteAsync(int id)
        {
            await _lostPropertyService.DeleteAsync(id);
            var deletedLostProperty = _lostProperties.FirstOrDefault(x => x.Id == id);
            Assert.True(deletedLostProperty.IsDelete);
        }

        [Test]
        public async Task HardDeleteAsync()
        {
            var lostProperty = new LostProperty
            {
                Id = 3
            };

            var preDeletedLostProperty = _lostProperties.FirstOrDefault(e => e.Id == lostProperty.Id);
            await _lostPropertyService.HardDeleteAsync(lostProperty.Id);
            var deletedLostProperty = _lostProperties.FirstOrDefault(x => x.Id == lostProperty.Id);
            Assert.True((preDeletedLostProperty is not null)
                && (deletedLostProperty is null));
        }
        #endregion

        #region Private member methods
        private ILostPropertyRepository SetUpLostPropertyRepository()
        {
            var mockRepo = new Mock<LostPropertyRepository>(MockBehavior.Default, _applicationDbContext);

            mockRepo.Setup(x => x.GetWithPaginationAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<LostProperty, bool>>>(),
                It.IsAny<Func<IQueryable<LostProperty>, IOrderedQueryable<LostProperty>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(
                    (int pageIndex,
                    int pageSize,
                    Expression<Func<LostProperty, bool>> filter,
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
                            ? orderBy(query)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                            : (IEnumerable<LostProperty>)query
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize);
                    }
                );

            mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Func<int, LostProperty>(id => _lostProperties.FirstOrDefault(e => e.Id == id)));

            mockRepo.Setup(p => p.InsertAsync((It.IsAny<LostProperty>())))
                .Callback(new Action<LostProperty>(newLostProperty =>
                {
                    int lastLostPropertyId = _lostProperties.Max(e => e.Id);
                    newLostProperty.Id = lastLostPropertyId + 1;
                    _lostProperties.Add(newLostProperty);
                }));

            mockRepo.Setup(x => x.UpdateAsync(It.IsAny<LostProperty>()))
                .Callback(new Action<LostProperty>(emp =>
                {
                    int oldLostProperty = _lostProperties.FindIndex(e => e.Id == emp.Id);
                    _lostProperties[oldLostProperty] = emp;
                }));

            mockRepo.Setup(x => x.DeleteAsync(It.IsAny<object>()))
                .Callback((object propertyId) =>
                {
                    int id = int.Parse(propertyId.ToString());
                    var propertyToRemove = _lostProperties.FirstOrDefault(e => e.Id == id);
                    if (propertyToRemove is not null)
                    {
                        propertyToRemove.IsDelete = true;
                    }
                });


            mockRepo.Setup(x => x.DeleteAsync(It.IsAny<LostProperty>()))
                 .Callback(new Action<LostProperty>(property =>
                 {
                     var propertyToRemove = _lostProperties.FirstOrDefault(e => e.Id == property.Id);
                     if (propertyToRemove is not null)
                     {
                         propertyToRemove.IsDelete = true;
                     }
                 }));

            mockRepo.Setup(x => x.HardDeleteAsync(It.IsAny<object>()))
                .Callback((object empId) =>
                {
                    int id = int.Parse(empId.ToString());
                    var lostPropertyToRemove = _lostProperties.FirstOrDefault(e => e.Id == id);
                    if (lostPropertyToRemove is not null)
                    {
                        _lostProperties.Remove(lostPropertyToRemove);
                    }
                });

            mockRepo.Setup(x => x.HardDeleteAsync(It.IsAny<LostProperty>()))
                .Callback(new Action<LostProperty>(property =>
                {
                    var lostPropertyToRemove = _lostProperties.FirstOrDefault(e => e.Id == property.Id);
                    if (lostPropertyToRemove is not null)
                    {
                        _lostProperties.Remove(lostPropertyToRemove);
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

                        if (orderBy is not null)
                        {
                            return orderBy(query)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize);
                        }
                        else
                        {
                            return query
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize);
                        }
                    }
                );

            mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Func<string, Employee>(id => _employees.FirstOrDefault(e => e.Id == id)));

            mockRepo.Setup(p => p.InsertAsync((It.IsAny<Employee>())))
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
            _lostPropertyService = null;
            _unitOfWork = null;
            _lostPropertyRepository = null;
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
