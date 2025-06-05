// tests/FitnessTrainerPro.Tests/ClientMeasurementModelTests.cs
using NUnit.Framework;
using FitnessTrainerPro.Core.Models;
using System;

namespace FitnessTrainerPro.Tests
{
    [TestFixture]
    public class ClientMeasurementModelTests
    {
        [Test]
        public void ClientMeasurement_CanBeCreatedAndPropertiesSet()
        {
            // Arrange
            var measurement = new ClientMeasurement
            {
                MeasurementID = 1,
                ClientID = 10,
                MeasurementDate = new DateTime(2023, 1, 15),
                WeightKg = 75.5,
                ChestCm = 100.2,
                WaistCm = 80.5,
                HipsCm = 102.1,
                Notes = "Первый замер после праздников",
                PhotoBeforePath = "/photos/client10/before_20230115.jpg",
                PhotoAfterPath = null // Может быть не указан
            };

            // Assert
            Assert.AreEqual(1, measurement.MeasurementID);
            Assert.AreEqual(10, measurement.ClientID);
            Assert.AreEqual(new DateTime(2023, 1, 15), measurement.MeasurementDate);
            Assert.AreEqual(75.5, measurement.WeightKg);
            Assert.AreEqual(100.2, measurement.ChestCm);
            Assert.AreEqual(80.5, measurement.WaistCm);
            Assert.AreEqual(102.1, measurement.HipsCm);
            Assert.AreEqual("Первый замер после праздников", measurement.Notes);
            Assert.AreEqual("/photos/client10/before_20230115.jpg", measurement.PhotoBeforePath);
            Assert.IsNull(measurement.PhotoAfterPath);
        }
    }
}