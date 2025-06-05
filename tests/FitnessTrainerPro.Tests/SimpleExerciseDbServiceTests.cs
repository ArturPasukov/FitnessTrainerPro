// FitnessTrainerPro.Tests/SimpleExerciseDbServiceTests.cs
using NUnit.Framework;
using Moq;
using Microsoft.EntityFrameworkCore;
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Data;
// using FitnessTrainerPro.Data.Services; // Зависит от того, где SimpleExerciseDbService
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking; // Для EntityEntry
using Microsoft.EntityFrameworkCore.Query;         // <--- ДОБАВЛЕНО ДЛЯ IAsyncQueryProvider

// Убедись, что SimpleExerciseDbService и ISimpleExerciseDbService доступны
// (например, если они в FitnessTrainerPro.Data/Services и FitnessTrainerPro.Core/Services)

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
                new Exercise { ExerciseID = 1, Name = "Приседания" },
                new Exercise { ExerciseID = 2, Name = "Жим лежа" }
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

            _mockDbSet.Setup(d => d.AddAsync(It.IsAny<Exercise>(), It.IsAny<CancellationToken>()))
                .Callback<Exercise, CancellationToken>((ex, ct) => _exerciseList.Add(ex))
                .ReturnsAsync((Exercise ex, CancellationToken ct) => 
                    {
                        // Создаем мок для EntityEntry<Exercise>
                        var mockEntry = new Mock<EntityEntry<Exercise>>(ex); // Передаем сущность в конструктор мока
                        return mockEntry.Object; // Возвращаем мокнутый EntityEntry
                    });

            _mockDbSet.Setup(d => d.Remove(It.IsAny<Exercise>()))
                .Callback<Exercise>(ex => 
                {
                    var itemToRemove = _exerciseList.FirstOrDefault(i => i.ExerciseID == ex.ExerciseID);
                    if (itemToRemove != null) _exerciseList.Remove(itemToRemove);
                });
            
            _mockDbContext = new Mock<FitnessDbContext>();
            _mockDbContext.Setup(c => c.Exercises).Returns(_mockDbSet.Object);
            _mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            
            _mockDbContext.Setup(c => c.Entry(It.IsAny<Exercise>())).Returns<Exercise>(entity => 
            {
                var mockEntry = new Mock<EntityEntry<Exercise>>(entity); // Передаем сущность
                mockEntry.Setup(e => e.State).Returns(EntityState.Unchanged); // Начальное состояние
                mockEntry.SetupSet(e => e.State = EntityState.Modified).Verifiable(); // Для отслеживания изменения состояния
                
                // Мок для CurrentValues.SetValues
                // Это сложнее, т.к. PropertyValues нелегко мокнуть.
                // Для простоты UpdateAsync в сервисе может напрямую изменять свойства `existing` объекта,
                // тогда этот сложный мок SetValues не так критичен, если мы проверяем сам _exerciseList.
                // Если сервис использует _context.Exercises.Update(exercise), то это проще мокнуть:
                // _mockDbSet.Setup(d => d.Update(It.IsAny<Exercise>())).Callback<Exercise>(ex => { /* логика обновления в _exerciseList */ });

                // Для варианта с _context.Entry(existing).CurrentValues.SetValues(exercise);
                // оставим как было, он обновляет _exerciseList через замыкание.
                mockEntry.Setup(e => e.CurrentValues.SetValues(It.IsAny<object>()))
                         .Callback<object>(obj => {
                             var updatedEx = obj as Exercise;
                             if (updatedEx != null) {
                                 var originalEx = _exerciseList.FirstOrDefault(e => e.ExerciseID == updatedEx.ExerciseID);
                                 if (originalEx != null) {
                                     originalEx.Name = updatedEx.Name; 
                                     originalEx.Description = updatedEx.Description;
                                     originalEx.MuscleGroup = updatedEx.MuscleGroup;
                                     originalEx.VideoUrl = updatedEx.VideoUrl;
                                     originalEx.EquipmentNeeded = updatedEx.EquipmentNeeded;
                                 }
                             }
                         });
                return mockEntry.Object;
            });

            // Предполагается, что SimpleExerciseDbService находится в FitnessTrainerPro.Data.Services
            _dbService = new SimpleExerciseDbService(_mockDbContext.Object); 
        }

        // ... (все твои существующие тесты AddAsync, DeleteAsync, UpdateAsync, GetAllAsync, GetByIdAsync) ...
        // Убедись, что они корректны, например:

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
            var exerciseToUpdate = new Exercise { ExerciseID = 1, Name = "Обновленные Приседания", Description = "С весом" };
            await _dbService.UpdateAsync(exerciseToUpdate);
            
            _mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            var updatedInList = _exerciseList.FirstOrDefault(e => e.ExerciseID == 1);
            Assert.IsNotNull(updatedInList);
            Assert.AreEqual("Обновленные Приседания", updatedInList.Name);
            Assert.AreEqual("С весом", updatedInList.Description);
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
            var executionResult = ((IQueryProvider)this).Execute(expression); // Синхронное выполнение

            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { executionResult })!;
        }
    }
}