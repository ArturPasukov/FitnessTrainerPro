// tests/FitnessTrainerPro.Tests/ExerciseFilterServiceTests.cs
using NUnit.Framework;
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Core.Services; // Наш новый сервис
using System.Collections.Generic;
using System.Linq;

namespace FitnessTrainerPro.Tests
{
    [TestFixture]
    public class ExerciseFilterServiceTests
    {
        private List<Exercise> _allExercises;

        [SetUp]
        public void Setup()
        {
            // Подготовка тестового набора упражнений
            _allExercises = new List<Exercise>
            {
                new Exercise { ExerciseID = 1, Name = "Жим лежа", MuscleGroup = "Грудь", EquipmentNeeded = "Штанга" },
                new Exercise { ExerciseID = 2, Name = "Приседания со штангой", MuscleGroup = "Ноги", EquipmentNeeded = "Штанга" },
                new Exercise { ExerciseID = 3, Name = "Подтягивания", MuscleGroup = "Спина", EquipmentNeeded = "Турник" },
                new Exercise { ExerciseID = 4, Name = "Отжимания от пола", MuscleGroup = "Грудь", EquipmentNeeded = "Нет" },
                new Exercise { ExerciseID = 5, Name = "Становая тяга", MuscleGroup = "Спина", EquipmentNeeded = "Штанга" }
            };
        }

        [Test]
        public void ApplyFilters_FilterByName_ReturnsMatchingExercises()
        {
            // Arrange
            var queryableExercises = _allExercises.AsQueryable();

            // Act
            var result = ExerciseFilterService.ApplyFilters(queryableExercises, "жим", null, null).ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Any(e => e.Name == "Жим лежа"));
        }

        [Test]
        public void ApplyFilters_FilterByMuscleGroup_ReturnsMatchingExercises()
        {
            // Arrange
            var queryableExercises = _allExercises.AsQueryable();

            // Act
            var result = ExerciseFilterService.ApplyFilters(queryableExercises, null, "спина", null).ToList();

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(e => e.MuscleGroup == "Спина"));
        }

        [Test]
        public void ApplyFilters_FilterByEquipment_ReturnsMatchingExercises()
        {
            // Arrange
            var queryableExercises = _allExercises.AsQueryable();

            // Act
            var result = ExerciseFilterService.ApplyFilters(queryableExercises, null, null, "штанга").ToList();

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.All(e => e.EquipmentNeeded == "Штанга"));
        }

        [Test]
        public void ApplyFilters_CombinedFilters_ReturnsMatchingExercises()
        {
            // Arrange
            var queryableExercises = _allExercises.AsQueryable();

            // Act
            var result = ExerciseFilterService.ApplyFilters(queryableExercises, "тяга", "спина", "штанга").ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Any(e => e.Name == "Становая тяга"));
        }

        [Test]
        public void ApplyFilters_NoMatchingFilters_ReturnsEmptyList()
        {
            // Arrange
            var queryableExercises = _allExercises.AsQueryable();

            // Act
            var result = ExerciseFilterService.ApplyFilters(queryableExercises, "несуществующее", null, null).ToList();

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public void ApplyFilters_AllFiltersNullOrEmpty_ReturnsAllExercises()
        {
            // Arrange
            var queryableExercises = _allExercises.AsQueryable();

            // Act
            var result = ExerciseFilterService.ApplyFilters(queryableExercises, null, "", "   ").ToList(); // null, пустая строка, пробелы

            // Assert
            Assert.AreEqual(_allExercises.Count, result.Count);
        }
    }
}