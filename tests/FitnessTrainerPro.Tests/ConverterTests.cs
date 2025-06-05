using NUnit.Framework;
using FitnessTrainerPro.UI.Converters; // Убедись, что путь к твоим конвертерам правильный
using System.Globalization;
using System; // Для typeof(bool)

namespace FitnessTrainerPro.Tests
{
    [TestFixture]
    public class ConverterTests
    {
        [Test]
        public void IsNullOrEmptyStringConverter_NullValue_ReturnsTrue()
        {
            // Arrange
            var converter = new IsNullOrEmptyStringConverter();
            string? testValue = null; // object value может быть null

            // Act
            // parameter и culture могут быть null, если конвертер их не использует
            bool result = (bool)converter.Convert(testValue, typeof(bool), null!, CultureInfo.InvariantCulture)!; 

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsNullOrEmptyStringConverter_EmptyStringValue_ReturnsTrue()
        {
            // Arrange
            var converter = new IsNullOrEmptyStringConverter();
            string testValue = "";

            // Act
            bool result = (bool)converter.Convert(testValue, typeof(bool), null!, CultureInfo.InvariantCulture)!;

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsNullOrEmptyStringConverter_NonEmptyStringValue_ReturnsFalse()
        {
            // Arrange
            var converter = new IsNullOrEmptyStringConverter();
            string testValue = "some text";

            // Act
            bool result = (bool)converter.Convert(testValue, typeof(bool), null!, CultureInfo.InvariantCulture)!;

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsNotNullOrEmptyToBoolConverter_NonEmptyString_ReturnsTrue()
        {
            // Arrange
            var converter = new IsNotNullOrEmptyToBoolConverter();
            string testValue = "hello";

            // Act
            bool result = (bool)converter.Convert(testValue, typeof(bool), null!, CultureInfo.InvariantCulture)!;

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsNotNullOrEmptyToBoolConverter_EmptyString_ReturnsFalse()
        {
            // Arrange
            var converter = new IsNotNullOrEmptyToBoolConverter();
            string testValue = "";

            // Act
            bool result = (bool)converter.Convert(testValue, typeof(bool), null!, CultureInfo.InvariantCulture)!;

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsNotNullOrEmptyToBoolConverter_NullString_ReturnsFalse() // Дополнительный тест для null
        {
            // Arrange
            var converter = new IsNotNullOrEmptyToBoolConverter();
            string? testValue = null;

            // Act
            bool result = (bool)converter.Convert(testValue, typeof(bool), null!, CultureInfo.InvariantCulture)!;

            // Assert
            Assert.IsFalse(result);
        }
    }
}