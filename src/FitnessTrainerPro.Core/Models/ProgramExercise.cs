// src/FitnessTrainerPro.Core/Models/ProgramExercise.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessTrainerPro.Core.Models
{
    public class ProgramExercise
    {
        [Key] // <--- ДОБАВЬ ЭТОТ АТРИБУТ
        public int ProgramExerciseID { get; set; }

        // ... остальные свойства ...
        [Required]
        public int ProgramID { get; set; }
        [Required]
        public int ExerciseID { get; set; }

        [Required(ErrorMessage = "Количество подходов обязательно")]
        [Range(1, 100, ErrorMessage = "Количество подходов должно быть от 1 до 100")]
        public int Sets { get; set; }

        [Required(ErrorMessage = "Количество повторений обязательно")]
        [StringLength(50, ErrorMessage = "Повторения не должны превышать 50 символов")]
        public string Reps { get; set; } = string.Empty; 

        [Range(0, 300, ErrorMessage = "Отдых должен быть от 0 до 300 секунд")]
        public int? RestSeconds { get; set; } 

        [StringLength(200, ErrorMessage = "Заметки не должны превышать 200 символов")]
        public string? Notes { get; set; }

        [Range(1, 100, ErrorMessage = "Порядок должен быть от 1 до 100")]
        public int OrderInProgram { get; set; } 

        [ForeignKey("ProgramID")]
        public virtual WorkoutProgram WorkoutProgram { get; set; } = null!;

        [ForeignKey("ExerciseID")]
        public virtual Exercise Exercise { get; set; } = null!;
    }
}