// FitnessTrainerPro.Tests/SimpleExerciseDbServiceTests.cs
using NUnit.Framework;
using Moq;
using Microsoft.EntityFrameworkCore;
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Data;
using FitnessTrainerPro.Core.Services;   // Для ISimpleExerciseDbService
using FitnessTrainerPro.Data.Services;   // Для SimpleExerciseDbService
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking; 
using Microsoft.EntityFrameworkCore.Query;         

namespace FitnessTrainerPro.Tests
{
    [TestFixture]
    public class SimpleExerciseDbServiceTests
    {
        private Mock<FitnessDbContext> _mockDbContext;
        private Mock<DbSet<Exercise>> _mockDbSet;
        private ISimpleExerciseDbService _dbService; 
        private List<Exercise> _exerciseList;

        [SetUp]
        public void Setup()
        {
            _exerciseList = new List<Exercise>
            {
                new Exercise { ExerciseID = 1, Name = "Приседания", Description = "Базовое" },
                new Exercise { ExerciseID = 2, Name = "Жим лежа", Description = "Для груди" }
            };

            _mockDbSet = new Mock<DbSet<Exercise>>();
            var queryableList = _exerciseList.AsQueryable();

            _mockDbSet.As<IQueryable<Exercise>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Exercise>(queryableList.Provider));
            _mockDbSet.As<IQueryable<Exercise>>().Setup(m => m.Expression).Returns(queryableList.Expression);
            _mockDbSet.As<IQueryable<Exercise>>().Setup(m => m.ElementType).Returns(queryableList.ElementType);
            _mockDbSet.As<IQueryable<Exercise>>().Setup(m => m.GetEnumerator()).Returns(() => _exerciseList.GetEnumerator());
            
            _mockDbSet.As<IAsyncEnumerable<Exercise>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Exercise>(_exerciseList.GetEnumerator()));

            _mockDbSet.Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .Returns<object[]>(ids => ValueTask.FromResult(_exerciseList.FirstOrDefault(e => e.ExerciseID == (int)ids[0])));

            // ИСПРАВЛЕННЫЙ МОК ДЛЯ AddAsync
            _mockDbSet.Setup(d => d.AddAsync(It.IsAny<Exercise>(), It.IsAny<CancellationToken>()))
                .Callback<Exercise, CancellationToken>((ex, ct) => _exerciseList.Add(ex))
                .Returns((Exercise ex, CancellationToken ct) =>
                {
                    var mockEntityEntry = new Mock<EntityEntry<Exercise>>();
                    mockEntityEntry.Setup(e => e.Entity).Returns(ex);
                    mockEntityEntry.Setup(e => e.State).Returns(EntityState.Added);
                    return ValueTask.FromResult(mockEntityEntry.Object);
                });


            _mockDbSet.Setup(d => d.Remove(It.IsAny<Exercise>()))
                .Callback<Exercise>(ex => 
                {
                    var itemToRemove = _exerciseList.FirstOrDefault(i => i.ExerciseID == ex.ExerciseID);
                    if (itemToRemove != null) _exerciseList.Remove(itemToRemove);
                });
            
            var options = new DbContextOptions<FitnessDbContext>(); 
            _mockDbContext = new Mock<FitnessDbContext>(options); 

            _mockDbContext.Setup(c => c.Exercises).Returns(_mockDbSet.Object);
            _mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            
            // Мок для Entry() нам не нужен, если UpdateAsync в сервисе напрямую меняет свойства existing
            _dbService = new SimpleExerciseDbService(_mockDbContext.Object); 
        }

        [Test]
        public async Task AddAsync_AddsExerciseToContext()
        {
            var newExercise = new Exercise { ExerciseID = 3, Name = "Новое" };
            await _dbService.AddAsync(newExercise);
            _mockDbSet.Verify(m => m.AddAsync(It.Is<Exercise>(e => e.ExerciseID == 3), It.IsAny<CancellationToken>()), Times.Once());
            _mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.IsTrue(_exerciseList.Any(e => e.ExerciseID == 3 && e.Name == "Новое"));
        }

        [Test]
        public async Task DeleteAsync_RemovesExerciseFromContext()
        {
            int idToDelete = 1;
            await _dbService.DeleteAsync(idToDelete);
            _mockDbSet.Verify(m => m.Remove(It.Is<Exercise>(e => e.ExerciseID == idToDelete)), Times.Once());
            _mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.IsFalse(_exerciseList.Any(e => e.ExerciseID == idToDelete));
        }
        
        [Test]
        public async Task UpdateAsync_ExerciseExists_UpdatesExerciseInContext()
        {
            var exerciseToUpdate = new Exercise { ExerciseID = 1, Name = "Обновленные Приседания", Description = "С большим весом" };
            await _dbService.UpdateAsync(exerciseToUpdate);
            
            _mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            var updatedInList = _exerciseList.FirstOrDefault(e => e.ExerciseID == 1);
            Assert.IsNotNull(updatedInList);
            Assert.AreEqual("Обновленные Приседания", updatedInList.Name);
            Assert.AreEqual("С большим весом", updatedInList.Description);
        }

         [Test]
        public async Task GetAllAsync_ReturnsAllExercisesFromList()
        {
            var result = await _dbService.GetAllAsync();
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(e => e.Name == "Приседания"));
            Assert.IsTrue(result.Any(e => e.Name == "Жим лежа"));
        }

        [Test]
        public async Task GetByIdAsync_ExerciseExists_ReturnsCorrectExercise()
        {
            var result = await _dbService.GetByIdAsync(1);
            Assert.IsNotNull(result);
            Assert.AreEqual("Приседания", result.Name);
        }

        [Test]
        public async Task GetByIdAsync_ExerciseDoesNotExist_ReturnsNull()
        {
            var result = await _dbService.GetByIdAsync(99);
            Assert.IsNull(result);
        }
    }

    // Вспомогательные классы для мокирования асинхронных операций EF Core
    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
        public T Current => _inner.Current;
        public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
        public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());
    }

    public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
        public TestAsyncEnumerable(Expression expression) : base(expression) { }
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }
        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;
        internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;
        public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
        public object? Execute(Expression expression) => _inner.Execute(expression);
        public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = ((IQueryProvider)this).Execute(expression); 

            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { executionResult })!;
        }
    }
}