// tests/FitnessTrainerPro.Tests/ProgramExerciseModelTests.cs
using NUnit.Framework;
using FitnessTrainerPro.Core.Models;

namespace FitnessTrainerPro.Tests
{
    [TestFixture]
    public class ProgramExerciseModelTests
    {
        [Test]
        public void ProgramExercise_CanBeCreatedAndPropertiesSet()
        {
            // Arrange
            var programExercise = new ProgramExercise
            {
                ProgramExerciseID = 1,
                ProgramID = 101,
                ExerciseID = 202,
                Sets = 3,
                Reps = "8-12",
                RestSeconds = 60,
                Notes = "Следить за техникой",
                OrderInProgram = 1
            };

            // Assert
            Assert.AreEqual(1, programExercise.ProgramExerciseID);
            Assert.AreEqual(101, programExercise.ProgramID);
            Assert.AreEqual(202, programExercise.ExerciseID);
            Assert.AreEqual(3, programExercise.Sets);
            Assert.AreEqual("8-12", programExercise.Reps);
            Assert.AreEqual(60, programExercise.RestSeconds);
            Assert.AreEqual("Следить за техникой", programExercise.Notes);
            Assert.AreEqual(1, programExercise.OrderInProgram);
        }
    }
}