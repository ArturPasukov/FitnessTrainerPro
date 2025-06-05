// tests/FitnessTrainerPro.Tests/ExerciseModelTests.cs
using NUnit.Framework;
using FitnessTrainerPro.Core.Models;

namespace FitnessTrainerPro.Tests
{
    [TestFixture]
    public class ExerciseModelTests
    {
        [Test]
        public void Exercise_CanBeCreatedAndPropertiesSet()
        {
            // Arrange
            var exercise = new Exercise
            {
                ExerciseID = 1,
                Name = "Test Exercise",
                Description = "Test Description",
                MuscleGroup = "Test Muscle",
                EquipmentNeeded = "Test Equipment",
                VideoUrl = "http://test.com"
            };

            // Assert
            Assert.AreEqual(1, exercise.ExerciseID);
            Assert.AreEqual("Test Exercise", exercise.Name);
            Assert.AreEqual("Test Description", exercise.Description);
            Assert.AreEqual("Test Muscle", exercise.MuscleGroup);
            Assert.AreEqual("Test Equipment", exercise.EquipmentNeeded);
            Assert.AreEqual("http://test.com", exercise.VideoUrl);
        }
    }
}