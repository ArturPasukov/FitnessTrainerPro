using NUnit.Framework;
using FitnessTrainerPro.UI.Converters; // Путь к твоим конвертерам
using System.Globalization; // Для CultureInfo

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
            string? testValue = null;

            // Act
            bool result = (bool)converter.Convert(testValue, typeof(bool), null, CultureInfo.CurrentCulture);

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
            bool result = (bool)converter.Convert(testValue, typeof(bool), null, CultureInfo.CurrentCulture);

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
            bool result = (bool)converter.Convert(testValue, typeof(bool), null, CultureInfo.CurrentCulture);

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
            bool result = (bool)converter.Convert(testValue, typeof(bool), null, CultureInfo.CurrentCulture);

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
            bool result = (bool)converter.Convert(testValue, typeof(bool), null, CultureInfo.CurrentCulture);

            // Assert
            Assert.IsFalse(result);
        }
    }
}