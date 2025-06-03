// src/FitnessTrainerPro.Core/Client.cs
namespace FitnessTrainerPro.Core.Models // Пространство имен по умолчанию для проекта Core
{
    public class Client
    {
        public int ClientID { get; set; } // EF Core по соглашению сделает это первичным ключом (PK)
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; } // <--- НОВОЕ СВОЙСТВО
        public string? Email { get; set; }       // <--- НОВОЕ СВОЙСТВО
        public string? Goals { get; set; }       // <--- НОВОЕ СВОЙСТВО
        // public string? Notes { get; set; } // Можно добавить и заметки позже
        // Добавим еще поля позже согласно ER-диаграмме
    }
}