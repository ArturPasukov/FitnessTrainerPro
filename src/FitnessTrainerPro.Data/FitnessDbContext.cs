// FitnessDbContext.cs
using Microsoft.EntityFrameworkCore;
using FitnessTrainerPro.Core.Models;
using System.IO; // <--- Добавь этот using для Path.Combine
using System;    // <--- Добавь этот using для Environment

namespace FitnessTrainerPro.Data
{
    public class FitnessDbContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Exercise> Exercises { get; set; }

        public FitnessDbContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Определяем путь к папке проекта. Это более надежно, чем жестко задавать.
                // AppContext.BaseDirectory вернет путь к папке bin/Debug/netX.X/ UI-проекта при запуске UI
                // Но для dotnet ef команд, лучше отталкиваться от чего-то более стабильного
                // Давай попробуем путь относительно папки решения или текущей директории,
                // но для максимальной надежности сейчас зададим явно.

                // Создадим папку Database в корне проекта, если ее нет
                string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..")); // Пытаемся подняться из bin/Debug/netX.X-windows/UI/src до корня проекта
                string dbFolderPath = Path.Combine(projectRootPath, "Database");
                
                // Если предыдущий способ определения projectRootPath не сработает корректно с dotnet ef,
                // можно временно захардкодить или использовать более простой относительный путь для dotnet ef,
                // например, создавая БД рядом с .sln файлом.
                // Для простоты отладки сейчас:
                dbFolderPath = @"C:\Users\lazaa\Desktop\proj1\FitnessTrainerPro\Database"; // <--- ЗАМЕНИ НА СВОЙ ТОЧНЫЙ ПУТЬ К ПАПКЕ ПРОЕКТА + \Database

                if (!Directory.Exists(dbFolderPath))
                {
                    Directory.CreateDirectory(dbFolderPath);
                }
                string dbPath = Path.Combine(dbFolderPath, "fitness_trainer_abs.db"); // Новое имя файла, чтобы не путаться

                optionsBuilder.UseSqlite($"Data Source={dbPath}");
                // Console.WriteLine($"Using database at: {dbPath}"); // Для отладки пути
            }
        }
    }
}