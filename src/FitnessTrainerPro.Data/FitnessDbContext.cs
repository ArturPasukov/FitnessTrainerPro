// src/FitnessTrainerPro.Data/FitnessDbContext.cs
using FitnessTrainerPro.Core.Models; // Чтобы видеть классы Client и Exercise
using Microsoft.EntityFrameworkCore;

namespace FitnessTrainerPro.Data
{
    public class FitnessDbContext : DbContext
    {
        // DbSet представляет коллекцию сущностей в контексте и, следовательно,
        // таблицу в базе данных.
        public DbSet<Client> Clients { get; set; }
        public DbSet<Exercise> Exercises { get; set; }

        // Этот конструктор нужен, если ты будешь настраивать DbContext извне,
        // например, при использовании Dependency Injection. Пока он нам не сильно нужен,
        // но лучше его оставить для будущего.
        // public FitnessDbContext(DbContextOptions<FitnessDbContext> options)
        //     : base(options)
        // {
        // }

        // Для простоты старта мы сконфигурируем строку подключения прямо здесь.
        // Этот метод вызывается EF Core при создании экземпляра DbContext, если он
        // не был сконфигурирован извне.
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Указываем, что будем использовать SQLite и имя файла базы данных.
                // Файл 'fitness_trainer.db' будет создан в папке вывода
                // стартового проекта (по умолчанию это FitnessTrainerPro.UI/bin/Debug/netX.X/)
                optionsBuilder.UseSqlite("Data Source=fitness_trainer.db");
            }
        }
    }
}