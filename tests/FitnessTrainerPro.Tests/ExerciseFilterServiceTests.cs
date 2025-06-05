// tests/FitnessTrainerPro.Tests/ExerciseFilterServiceTests.cs
using NUnit.Framework;
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Core.Services;
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
            _allExercises = new List<Exercise>
            {
                new Exercise { ExerciseID = 1, Name = "Классический Жим Лежа", MuscleGroup = "Грудь", EquipmentNeeded = "Штанга" },
                new Exercise { ExerciseID = 2, Name = "Приседания со штангой", MuscleGroup = "Ноги", EquipmentNeeded = "Штанга" },
                new Exercise { ExerciseID = 3, Name = "Подтягивания", MuscleGroup = "Спина", EquipmentNeeded = "Турник" },
                new Exercise { ExerciseID = 4, Name = "Отж от пола (грудные)", MuscleGroup = "Грудь", EquipmentNeeded = "Нет" },
                new Exercise { ExerciseID = 5, Name = "Становая тяга", MuscleGroup = "Спина", EquipmentNeeded = "Штанга" },
                new Exercise { ExerciseID = 6, Name = "Жим гантелей сидя", MuscleGroup = "Плечи", EquipmentNeeded = "Гантели" } 
            };
        }

        [Test]
        public void ApplyFilters_FilterByName_ExactMatch_ReturnsOne()
        {
            var queryableExercises = _allExercises.AsQueryable();
            var result = ExerciseFilterService.ApplyFilters(queryableExercises, "Классический Жим Лежа", null, null).ToList();
            Assert.AreEqual(1, result.Count, "Должно найтись только 'Классический Жим Лежа'");
            Assert.AreEqual("Классический Жим Лежа", result.First().Name);
        }
        
        [Test]
        public void ApplyFilters_FilterByName_PartialWordLowerCase_FindsTwo()
        {
            var queryableExercises = _allExercises.AsQueryable();
            var result = ExerciseFilterService.ApplyFilters(queryableExercises, "жим", null, null).ToList();
            Assert.AreEqual(2, result.Count, "Должно найтись ДВА упражнения, содержащих 'жим' без учета регистра");
            Assert.IsTrue(result.Any(e => e.Name == "Классический Жим Лежа"));
            Assert.IsTrue(result.Any(e => e.Name == "Жим гантелей сидя"));
        }
        
        [Test]
        public void ApplyFilters_FilterByName_PartialWordUpperCase_FindsTwo()
        {
            var queryableExercises = _allExercises.AsQueryable();
            var result = ExerciseFilterService.ApplyFilters(queryableExercises, "ЖИМ", null, null).ToList();
            Assert.AreEqual(2, result.Count, "Должно найтись ДВА упражнения, содержащих 'ЖИМ' без учета регистра");
            Assert.IsTrue(result.Any(e => e.Name == "Классический Жим Лежа"));
            Assert.IsTrue(result.Any(e => e.Name == "Жим гантелей сидя"));
        }

        [Test]
        public void ApplyFilters_FilterByMuscleGroup_ReturnsMatchingExercises()
        {
            var queryableExercises = _allExercises.AsQueryable();
            var result = ExerciseFilterService.ApplyFilters(queryableExercises, null, "спина", null).ToList(); 
            Assert.AreEqual(2, result.Count); 
            Assert.IsTrue(result.Any(e => e.Name == "Подтягивания"));
            Assert.IsTrue(result.Any(e => e.Name == "Становая тяга"));
        }

        [Test]
        public void ApplyFilters_FilterByEquipment_ReturnsMatchingExercises()
        {
            var queryableExercises = _allExercises.AsQueryable();
            var result = ExerciseFilterService.ApplyFilters(queryableExercises, null, null, "штанга").ToList();
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.All(e => e.EquipmentNeeded != null && e.EquipmentNeeded.ToLower().Contains("штанга")));
        }

        [Test]
        public void ApplyFilters_CombinedFilters_ReturnsMatchingExercises()
        {
            var queryableExercises = _allExercises.AsQueryable();
            var result = ExerciseFilterService.ApplyFilters(queryableExercises, "тяга", "спина", "штанга").ToList();
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Any(e => e.Name == "Становая тяга"));
        }

        [Test]
        public void ApplyFilters_NoMatchingFilters_ReturnsEmptyList()
        {
            var queryableExercises = _allExercises.AsQueryable();
            var result = ExerciseFilterService.ApplyFilters(queryableExercises, "несуществующее", null, null).ToList();
            Assert.IsEmpty(result);
        }

        [Test]
        public void ApplyFilters_AllFiltersNullOrEmpty_ReturnsAllExercises()
        {
            var queryableExercises = _allExercises.AsQueryable();
            var result = ExerciseFilterService.ApplyFilters(queryableExercises, null, "", "   ").ToList();
            Assert.AreEqual(_allExercises.Count, result.Count);
        }
    }
}