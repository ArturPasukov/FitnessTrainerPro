// src/FitnessTrainerPro.Core/Models/Client.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // Для [NotMapped]

namespace FitnessTrainerPro.Core.Models
{
    public class Client
    {
        public int ClientID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Goals { get; set; }
        
        public virtual ICollection<ClientAssignedProgram> AssignedPrograms { get; set; } = new List<ClientAssignedProgram>();
        public virtual ICollection<ClientMeasurement> Measurements { get; set; } = new List<ClientMeasurement>();

        // НОВОЕ СВОЙСТВО ТОЛЬКО ДЛЯ ЧТЕНИЯ (не сохраняется в БД)
        [NotMapped] // Атрибут, чтобы EF Core не пытался создать столбец для этого свойства
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}