// tests/FitnessTrainerPro.Tests/SimpleClientDbServiceTests.cs
using NUnit.Framework;
using Moq;
using Microsoft.EntityFrameworkCore;
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Data;
using FitnessTrainerPro.Core.Services;
using FitnessTrainerPro.Data.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query; // Для TestAsyncQueryProvider
using System; // Для DateTime

namespace FitnessTrainerPro.Tests
{
    [TestFixture]
    public class SimpleClientDbServiceTests
    {
        private Mock<FitnessDbContext> _mockDbContext;
        private Mock<DbSet<Client>> _mockDbSet;
        private ISimpleClientDbService _clientService;
        private List<Client> _clientList;

        [SetUp]
        public void Setup()
        {
            _clientList = new List<Client>
            {
                new Client { ClientID = 1, FirstName = "Иван", LastName = "Петров", DateOfBirth = new DateTime(1990, 1, 1) },
                new Client { ClientID = 2, FirstName = "Анна", LastName = "Сидорова", DateOfBirth = new DateTime(1995, 5, 15) }
            };

            _mockDbSet = new Mock<DbSet<Client>>();
            var queryableList = _clientList.AsQueryable();

            _mockDbSet.As<IQueryable<Client>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Client>(queryableList.Provider));
            _mockDbSet.As<IQueryable<Client>>().Setup(m => m.Expression).Returns(queryableList.Expression);
            _mockDbSet.As<IQueryable<Client>>().Setup(m => m.ElementType).Returns(queryableList.ElementType);
            _mockDbSet.As<IQueryable<Client>>().Setup(m => m.GetEnumerator()).Returns(() => _clientList.GetEnumerator());
            
            _mockDbSet.As<IAsyncEnumerable<Client>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Client>(_clientList.GetEnumerator()));

            _mockDbSet.Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .Returns<object[]>(ids => ValueTask.FromResult(_clientList.FirstOrDefault(c => c.ClientID == (int)ids[0])));

            _mockDbSet.Setup(d => d.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .Callback<Client, CancellationToken>((client, ct) => _clientList.Add(client))
                .Returns(ValueTask.FromResult<EntityEntry<Client>>(result: null!)); // Упрощенный возврат

            _mockDbSet.Setup(d => d.Remove(It.IsAny<Client>()))
                .Callback<Client>(client => 
                {
                    var itemToRemove = _clientList.FirstOrDefault(c => c.ClientID == client.ClientID);
                    if (itemToRemove != null) _clientList.Remove(itemToRemove);
                });
            
            var options = new DbContextOptions<FitnessDbContext>(); 
            _mockDbContext = new Mock<FitnessDbContext>(options); 

            _mockDbContext.Setup(c => c.Clients).Returns(_mockDbSet.Object); // Важно: Clients, а не Exercises
            _mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            
            _clientService = new SimpleClientDbService(_mockDbContext.Object); 
        }

        [Test]
        public async Task GetAllClientsAsync_ReturnsAllClients()
        {
            var result = await _clientService.GetAllClientsAsync();
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(c => c.FirstName == "Иван"));
        }

        [Test]
        public async Task GetClientByIdAsync_ClientExists_ReturnsClient()
        {
            var result = await _clientService.GetClientByIdAsync(1);
            Assert.IsNotNull(result);
            Assert.AreEqual("Иван", result.FirstName);
        }

        [Test]
        public async Task AddClientAsync_AddsClientSuccessfully()
        {
            var newClient = new Client { ClientID = 3, FirstName = "Сергей", LastName = "Иванов" };
            await _clientService.AddClientAsync(newClient);

            _mockDbSet.Verify(m => m.AddAsync(It.Is<Client>(c => c.ClientID == 3), It.IsAny<CancellationToken>()), Times.Once());
            _mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.IsTrue(_clientList.Any(c => c.ClientID == 3));
        }

        [Test]
        public async Task UpdateClientAsync_ClientExists_UpdatesClient()
        {
            var clientToUpdate = new Client { ClientID = 1, FirstName = "Иван-Обновленный", LastName = "Петров" };
            await _clientService.UpdateClientAsync(clientToUpdate);

            _mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            var updatedInList = _clientList.FirstOrDefault(c => c.ClientID == 1);
            Assert.IsNotNull(updatedInList);
            Assert.AreEqual("Иван-Обновленный", updatedInList.FirstName);
        }

        [Test]
        public async Task DeleteClientAsync_ClientExists_RemovesClient()
        {
            await _clientService.DeleteClientAsync(1);

            _mockDbSet.Verify(m => m.Remove(It.Is<Client>(c => c.ClientID == 1)), Times.Once());
            _mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.IsFalse(_clientList.Any(c => c.ClientID == 1));
        }
    }
    // Вспомогательные классы TestAsyncEnumerator, TestAsyncEnumerable, TestAsyncQueryProvider 
    // должны быть уже определены в этом проекте (например, в SimpleExerciseDbServiceTests.cs или отдельном файле)
    // Если нет, скопируй их определения сюда.
}