// tests/FitnessTrainerPro.Tests/ClientAssignedProgramModelTests.cs
using NUnit.Framework;
using FitnessTrainerPro.Core.Models;
using System;

namespace FitnessTrainerPro.Tests
{
    [TestFixture]
    public class ClientAssignedProgramModelTests
    {
        [Test]
        public void ClientAssignedProgram_CanBeCreatedAndPropertiesSet()
        {
            // Arrange
            var assignment = new ClientAssignedProgram
            {
                ClientAssignedProgramID = 1,
                ClientID = 10,
                ProgramID = 101,
                StartDate = new DateTime(2023, 6, 1),
                EndDate = new DateTime(2023, 9, 1),
                IsActive = true,
                TrainerNotesForClient = "Начать с легких весов"
            };

            // Assert
            Assert.AreEqual(1, assignment.ClientAssignedProgramID);
            Assert.AreEqual(10, assignment.ClientID);
            Assert.AreEqual(101, assignment.ProgramID);
            Assert.AreEqual(new DateTime(2023, 6, 1), assignment.StartDate);
            Assert.AreEqual(new DateTime(2023, 9, 1), assignment.EndDate);
            Assert.IsTrue(assignment.IsActive);
            Assert.AreEqual("Начать с легких весов", assignment.TrainerNotesForClient);
        }
    }
}