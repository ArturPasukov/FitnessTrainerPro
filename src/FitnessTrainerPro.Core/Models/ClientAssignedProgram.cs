// src/FitnessTrainerPro.Core/Models/ClientAssignedProgram.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessTrainerPro.Core.Models
{
    public class ClientAssignedProgram
    {
        [Key] // <--- ДОБАВЬ ЭТОТ АТРИБУТ
        public int ClientAssignedProgramID { get; set; }

        // ... остальные свойства ...
        [Required]
        public int ClientID { get; set; }
        [Required]
        public int ProgramID { get; set; }

        [Required(ErrorMessage = "Дата начала обязательна")]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        [StringLength(500, ErrorMessage = "Заметки тренера не должны превышать 500 символов")]
        public string? TrainerNotesForClient { get; set; }

        [ForeignKey("ClientID")]
        public virtual Client Client { get; set; } = null!;

        [ForeignKey("ProgramID")]
        public virtual WorkoutProgram WorkoutProgram { get; set; } = null!;
    }
}