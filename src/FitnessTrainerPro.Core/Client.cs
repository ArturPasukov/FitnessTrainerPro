// src/FitnessTrainerPro.Core/Models/Client.cs
using System;
using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations;

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

        // НОВОЕ НАВИГАЦИОННОЕ СВОЙСТВО
        public virtual ICollection<ClientMeasurement> Measurements { get; set; } = new List<ClientMeasurement>();
    }
}