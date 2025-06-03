// src/FitnessTrainerPro.Core/Models/WorkoutProgram.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Убедись, что этот using есть

namespace FitnessTrainerPro.Core.Models
{
    public class WorkoutProgram
    {
        [Key] // <--- ДОБАВЬ ЭТОТ АТРИБУТ
        public int ProgramID { get; set; }

        [Required(ErrorMessage = "Название программы обязательно")]
        [StringLength(100, ErrorMessage = "Название программы не должно превышать 100 символов")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "Направленность не должна превышать 50 символов")]
        public string? Focus { get; set; }

        public virtual ICollection<ProgramExercise> ProgramExercises { get; set; } = new List<ProgramExercise>();
        public virtual ICollection<ClientAssignedProgram> ClientAssignments { get; set; } = new List<ClientAssignedProgram>();
    }
}