// src/FitnessTrainerPro.Core/Models/ClientMeasurement.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessTrainerPro.Core.Models
{
    public class ClientMeasurement
    {
        [Key]
        public int MeasurementID { get; set; }

        [Required]
        public int ClientID { get; set; } // Внешний ключ к Client

        [Required(ErrorMessage = "Дата замера обязательна")]
        public DateTime MeasurementDate { get; set; } = DateTime.Today;

        [Range(0, 500, ErrorMessage = "Вес должен быть положительным числом")]
        public double? WeightKg { get; set; } // Вес в кг

        [Range(0, 300, ErrorMessage = "Объем груди должен быть положительным числом")]
        public double? ChestCm { get; set; } // Обхват груди в см

        [Range(0, 300, ErrorMessage = "Объем талии должен быть положительным числом")]
        public double? WaistCm { get; set; } // Обхват талии в см

        [Range(0, 300, ErrorMessage = "Объем бедер должен быть положительным числом")]
        public double? HipsCm { get; set; } // Обхват бедер в см
        
        [StringLength(500)]
        public string? Notes { get; set; } // Дополнительные заметки

        // НОВЫЕ СВОЙСТВА ДЛЯ ПУТЕЙ К ФОТОГРАФИЯМ
        [StringLength(260)] // Стандартная максимальная длина пути в Windows, можно увеличить при необходимости
        public string? PhotoBeforePath { get; set; }

        [StringLength(260)]
        public string? PhotoAfterPath { get; set; }
        // КОНЕЦ НОВЫХ СВОЙСТВ

        // Навигационное свойство
        [ForeignKey("ClientID")]
        public virtual Client Client { get; set; } = null!;
    }
}