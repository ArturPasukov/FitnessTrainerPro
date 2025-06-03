// src/FitnessTrainerPro.Core/Models/Client.cs
using System;
using System.Collections.Generic; // <--- ДОБАВЬ ЭТОТ USING, ЕСЛИ ЕГО НЕТ
// using System.ComponentModel.DataAnnotations; // Пока не используем здесь, но может понадобиться

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
        
        // НОВОЕ НАВИГАЦИОННОЕ СВОЙСТВО
        public virtual ICollection<ClientAssignedProgram> AssignedPrograms { get; set; } = new List<ClientAssignedProgram>();
    }
}