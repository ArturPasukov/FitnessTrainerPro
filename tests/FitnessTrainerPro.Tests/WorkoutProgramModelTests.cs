// tests/FitnessTrainerPro.Tests/WorkoutProgramModelTests.cs
using NUnit.Framework;
using FitnessTrainerPro.Core.Models;
using System.Collections.Generic; // Для инициализации коллекций

namespace FitnessTrainerPro.Tests
{
    [TestFixture]
    public class WorkoutProgramModelTests
    {
        [Test]
        public void WorkoutProgram_CanBeCreatedAndPropertiesSet()
        {
            // Arrange
            var program = new WorkoutProgram
            {
                ProgramID = 1,
                Name = "Test Program",
                Description = "Test Desc",
                Focus = "Test Focus",
                ProgramExercises = new List<ProgramExercise>(), // Инициализируем, чтобы не было null
                ClientAssignments = new List<ClientAssignedProgram>() // Инициализируем
            };

            // Assert
            Assert.AreEqual(1, program.ProgramID);
            Assert.AreEqual("Test Program", program.Name);
            Assert.AreEqual("Test Desc", program.Description);
            Assert.AreEqual("Test Focus", program.Focus);
            Assert.IsNotNull(program.ProgramExercises);
            Assert.IsNotNull(program.ClientAssignments);
        }
    }
}