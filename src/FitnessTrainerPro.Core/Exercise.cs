// src/FitnessTrainerPro.Core/Exercise.cs
namespace FitnessTrainerPro.Core.Models
{
    public class Exercise
    {
        public int ExerciseID { get; set; } // PK
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? VideoUrl { get; set; } // Поле из задания
        public string? MuscleGroup { get; set; } // Поле из задания
        public string? EquipmentNeeded { get; set; } // Поле из задания
    }
}