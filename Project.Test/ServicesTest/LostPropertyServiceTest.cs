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

namespace Project.Test.ServicesTest
{
    public class LostPropertyServiceTest
    {
        #region Variables
        private ILostPropertyService _lostPropertyService;
        private IUnitOfWork _unitOfWork;
        private List<LostProperty> _lostProperties;
        private ILostPropertyRepository _lostPropertyRepository;
        private IApplicationDbContext _applicationDbContext;
        #endregion

        #region Test fixture setup
        [OneTimeSetUp]
        public void SetUp()
        {
            _lostProperties = DataInitializer.GetAllLostProperties();
        }
        #endregion

        #region Setup
        [SetUp]
        public void ReInitializeTest()
        {
            _lostProperties = DataInitializer.GetAllLostProperties();
            _applicationDbContext = new Mock<IApplicationDbContext>().Object;
            _lostPropertyRepository = SetUpLostPropertyRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.LostPropertyRepository).Returns(_lostPropertyRepository);
            _unitOfWork = unitOfWork.Object;
            _lostPropertyService = new LostPropertyService(unitOfWork.Object);
        }
        #endregion

        #region Unit Tests
        [Test]
        public async Task GetAllAsync_ReturnCollection()
        {
            var lostProperties = await _lostPropertyService.GetAllAsync();
            CollectionAssert.AreEqual(lostProperties, _lostProperties);
        }

        [Test]
        [TestCase("0")]
        public async Task GetByIdAsync_ReturnLostProperty(int id)
        {
            var lostProperty = await _lostPropertyService.GetByIdAsync(id);
            Assert.AreEqual(_lostProperties[0], lostProperty);
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

            };

            await _lostPropertyService.AddAsync(insertedLostProperty);
            Assert.AreEqual(_lostProperties.Last(), insertedLostProperty);
        }

        [Test]
        public async Task UpdateAsync()
        {
            var lostProperty = new LostProperty
            {

            };

            var updatedLostProperty = new LostProperty
            {

            };

            await _lostPropertyService.UpdateAsync(updatedLostProperty);
            var changedLostProperty = _lostProperties.Find(x => x.Id == lostProperty.Id);

        }

        [Test]
        public async Task DeleteAsync()
        {
            var lostProperty = new LostProperty
            {

            };

            await _lostPropertyService.DeleteAsync(lostProperty.Id);
            var deletedLostProperty = _lostProperties.Find(x => x.Id == lostProperty.Id);
            Assert.True(deletedLostProperty.IsDelete);
        }

        [Test]
        public async Task HardDeleteAsync()
        {
            var lostProperty = new LostProperty
            {
                Id = 3
            };

            var preDeletedLostProperty = _lostProperties.Find(e => e.Id == lostProperty.Id);
            await _lostPropertyService.HardDeleteAsync(lostProperty.Id);
            var deletedLostProperty = _lostProperties.Find(x => x.Id == lostProperty.Id);
            Assert.True((preDeletedLostProperty != null)
                && (deletedLostProperty == null));
        }
        #endregion

        #region Private member methods
        private ILostPropertyRepository SetUpLostPropertyRepository()
        {
            var mockRepo = new Mock<LostPropertyRepository>(MockBehavior.Default, _applicationDbContext);

            mockRepo.Setup(x => x.GetAsync(It.IsAny<Expression<Func<LostProperty, bool>>>()
                , It.IsAny<Func<IQueryable<LostProperty>, IOrderedQueryable<LostProperty>>>()
                , It.IsAny<string>()))
                .ReturnsAsync(_lostProperties);

            mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Func<int, LostProperty>(id => _lostProperties.Find(e => e.Id.Equals(id))));

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
                    int oldLostProperty = _lostProperties.FindIndex(e => e.Id.Equals(emp.Id));
                    _lostProperties[oldLostProperty] = emp;
                }));

            mockRepo.Setup(x => x.DeleteAsync(It.IsAny<object>()))
                .Callback((object propertyId) =>
                {
                    int id = int.Parse(propertyId.ToString());
                    var lostPropertyToRemove = _lostProperties.Find(e => e.Id.Equals(propertyId));
                    if (lostPropertyToRemove != null)
                    {
                        lostPropertyToRemove.IsDelete = true;
                    }
                });


            mockRepo.Setup(x => x.DeleteAsync(It.IsAny<LostProperty>()))
                .Callback(new Action<LostProperty>(emp =>
                {
                    var lostPropertyToRemove = _lostProperties.Find(e => e.Id.Equals(emp.Id));
                    if (lostPropertyToRemove != null)
                    {
                        lostPropertyToRemove.IsDelete = true;
                    }
                }));

            mockRepo.Setup(x => x.HardDeleteAsync(It.IsAny<object>()))
                .Callback((object empId) =>
                {
                    int id = int.Parse(empId.ToString());
                    var lostPropertyToRemove = _lostProperties.Find(e => e.Id.Equals(id));
                    if (lostPropertyToRemove != null)
                    {
                        _lostProperties.Remove(lostPropertyToRemove);
                    }
                });

            mockRepo.Setup(x => x.HardDeleteAsync(It.IsAny<LostProperty>()))
                .Callback(new Action<LostProperty>(emp =>
                {
                    var lostPropertyToRemove = _lostProperties.Find(e => e.Id.Equals(emp.Id));
                    if (lostPropertyToRemove != null)
                    {
                        _lostProperties.Remove(lostPropertyToRemove);
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
            if (_applicationDbContext != null)
            {
                _applicationDbContext.Dispose();
            }
            _lostProperties = null;
        }
        #endregion

        #region TestFixture TearDown
        [OneTimeTearDown]
        public void DisposeAllObjects()
        {
            _lostProperties = null;
        }
        #endregion
    }
}
