using NUnit.Framework;
using FitnessTrainerPro.Core.Models; // Убедись, что это пространство имен твоих моделей

namespace FitnessTrainerPro.Tests
{
    [TestFixture]
    public class ClientModelTests
    {
        [Test]
        public void FullName_WithValidFirstAndLastName_ReturnsCorrectFullName()
        {
            // Arrange
            var client = new Client { FirstName = "Артур", LastName = "Пасуков" };

            // Act
            string result = client.FullName;

            // Assert
            Assert.AreEqual("Артур Пасуков", result);
        }

        [Test]
        public void FullName_WithFirstNameOnly_ReturnsFirstName()
        {
            // Arrange
            var client = new Client { FirstName = "Артур", LastName = null }; // Или LastName = ""

            // Act
            string result = client.FullName;

            // Assert
            Assert.AreEqual("Артур", result);
        }

        [Test]
        public void FullName_WithLastNameOnly_ReturnsLastName()
        {
            // Arrange
            var client = new Client { FirstName = string.Empty, LastName = "Пасуков" }; // Или FirstName = null

            // Act
            string result = client.FullName;

            // Assert
            Assert.AreEqual("Пасуков", result);
        }

        [Test]
        public void FullName_WithBothNamesNull_ReturnsEmptyString()
        {
            // Arrange
            var client = new Client { FirstName = null, LastName = null };

            // Act
            string result = client.FullName;

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void FullName_WithBothNamesEmpty_ReturnsEmptyString()
        {
            // Arrange
            var client = new Client { FirstName = "", LastName = string.Empty };

            // Act
            string result = client.FullName;

            // Assert
            Assert.AreEqual(string.Empty, result);
        }
    }
}