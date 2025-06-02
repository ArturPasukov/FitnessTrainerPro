// src/FitnessTrainerPro.Core/Client.cs
namespace FitnessTrainerPro.Core.Models // Пространство имен по умолчанию для проекта Core
{
    public class Client
    {
        public int ClientID { get; set; } // EF Core по соглашению сделает это первичным ключом (PK)
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        // Добавим еще поля позже согласно ER-диаграмме
    }
}