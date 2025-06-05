// FitnessTrainerPro.Tests/SimpleExerciseDbServiceTests.cs
using NUnit.Framework;
using Moq;
using Microsoft.EntityFrameworkCore;
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Data;
// Убедись, что using для твоего сервиса (SimpleExerciseDbService) правильный
// Например, если он в FitnessTrainerPro.Data.Services:
// using FitnessTrainerPro.Data.Services; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking; 
using Microsoft.EntityFrameworkCore.Query;         

// ПРЕДПОЛАГАЕТСЯ, ЧТО КЛАССЫ ISimpleExerciseDbService и SimpleExerciseDbService УЖЕ СУЩЕСТВУЮТ
// ISimpleExerciseDbService в FitnessTrainerPro.Core/Services/
// SimpleExerciseDbService в FitnessTrainerPro.Data/Services/
// И что SimpleExerciseDbService.UpdateAsync ИЗМЕНЕН, как я предлагал (копирует свойства напрямую).

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

            // ИЗМЕНЕННЫЙ МОК ДЛЯ AddAsync
            _mockDbSet.Setup(d => d.AddAsync(It.IsAny<Exercise>(), It.IsAny<CancellationToken>()))
                .Callback<Exercise, CancellationToken>((ex, ct) => _exerciseList.Add(ex))
                .Returns((Exercise ex, CancellationToken ct) => 
                    // Возвращаем ValueTask<EntityEntry<Exercise>>. 
                    // Для простоты можно создать мок EntityEntry без сложной настройки, если его результат не используется.
                    // Либо, если EntityEntry не используется вообще после вызова AddAsync в сервисе, можно вернуть ValueTask.FromResult<EntityEntry<Exercise>>(null!).
                    // Но чтобы избежать ошибки "Can not instantiate proxy", лучше не пытаться мокать его через new Mock<EntityEntry<Exercise>>(ex) без нужных зависимостей.
                    // Простой мок EntityEntry:
                    ValueTask.FromResult(Mock.Of<EntityEntry<Exercise>>(e => e.Entity == ex && e.State == EntityState.Added)));


            _mockDbSet.Setup(d => d.Remove(It.IsAny<Exercise>()))
                .Callback<Exercise>(ex => 
                {
                    var itemToRemove = _exerciseList.FirstOrDefault(i => i.ExerciseID == ex.ExerciseID);
                    if (itemToRemove != null) _exerciseList.Remove(itemToRemove);
                });
            
            // _mockDbContext = new Mock<FitnessDbContext>(); // Это если FitnessDbContext имеет конструктор без параметров или который Moq может вызвать
            // Если FitnessDbContext требует DbContextOptions, но ты хочешь мокать все его методы, то можно так:
            var options = new DbContextOptions<FitnessDbContext>(); // Фиктивные опции
            _mockDbContext = new Mock<FitnessDbContext>(options); // Передаем фиктивные опции

            _mockDbContext.Setup(c => c.Exercises).Returns(_mockDbSet.Object);
            _mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            
            // УДАЛЕН СЛОЖНЫЙ МОК ДЛЯ Entry(), так как мы изменили логику UpdateAsync в сервисе
            // _mockDbContext.Setup(c => c.Entry(It.IsAny<Exercise>())).Returns<Exercise>(entity => ... );

            // Убедись, что класс SimpleExerciseDbService существует и доступен
            // (например, в проекте FitnessTrainerPro.Data.Services)
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
            // Для Remove передается сам объект, а не только ID
            var exerciseInstanceToRemove = _exerciseList.First(e => e.ExerciseID == idToDelete); 

            await _dbService.DeleteAsync(idToDelete); // Сервис найдет объект по ID и вызовет Remove

            // Проверяем, что Remove был вызван на DbSet с объектом, имеющим правильный ID
            _mockDbSet.Verify(m => m.Remove(It.Is<Exercise>(e => e.ExerciseID == idToDelete)), Times.Once());
            _mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.IsFalse(_exerciseList.Any(e => e.ExerciseID == idToDelete));
        }
        
        [Test]
        public async Task UpdateAsync_ExerciseExists_UpdatesExerciseInContext()
        {
            // Создаем объект с обновленными данными
            var exerciseToUpdate = new Exercise { ExerciseID = 1, Name = "Обновленные Приседания", Description = "С большим весом" };
            
            // Моделируем, что FindAsync вернет существующий объект, который затем будет изменен
            var originalExerciseInList = _exerciseList.First(e => e.ExerciseID == exerciseToUpdate.ExerciseID);
            // Важно, чтобы FindAsync возвращал _копию_ или чтобы сервис работал с загруженным из контекста объектом.
            // В нашем моке FindAsync возвращает объект из _exerciseList.
            // SimpleExerciseDbService.UpdateAsync изменяет свойства этого же объекта.

            await _dbService.UpdateAsync(exerciseToUpdate);
            
            _mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            
            var updatedInList = _exerciseList.FirstOrDefault(e => e.ExerciseID == 1);
            Assert.IsNotNull(updatedInList);
            Assert.AreEqual("Обновленные Приседания", updatedInList.Name);
            Assert.AreEqual("С большим весом", updatedInList.Description); // Проверяем обновленное описание
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
    // (TestAsyncEnumerator, TestAsyncEnumerable, TestAsyncQueryProvider остаются такими же, как я приводил ранее)
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