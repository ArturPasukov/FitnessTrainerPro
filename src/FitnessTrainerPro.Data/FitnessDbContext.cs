// FitnessDbContext.cs
using Microsoft.EntityFrameworkCore;
using FitnessTrainerPro.Core.Models;
using System.IO; 
using System;    

namespace FitnessTrainerPro.Data
{
    public class FitnessDbContext : DbContext
    {
        // DbSet'ы должны быть virtual для возможности мокирования в тестах
        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<Exercise> Exercises { get; set; }
        public virtual DbSet<WorkoutProgram> WorkoutPrograms { get; set; }
        public virtual DbSet<ProgramExercise> ProgramExercises { get; set; }
        public virtual DbSet<ClientAssignedProgram> ClientAssignedPrograms { get; set; }
        public virtual DbSet<ClientMeasurement> ClientMeasurements { get; set; }

        // --- КОНСТРУКТОРЫ ---
        // 1. Публичный конструктор без параметров.
        // Он нужен для:
        //    - Инструментов EF Core (например, для создания миграций, если не используется DI).
        //    - Простого создания экземпляра `new FitnessDbContext()` в твоем UI-коде.
        //    - Moq в тестах также сможет его использовать.
        public FitnessDbContext() { } 

        // 2. Публичный конструктор, принимающий DbContextOptions.
        // Он нужен для:
        //    - Настройки DbContext при использовании Dependency Injection (рекомендуемый подход в больших приложениях).
        //    - Создания экземпляра DbContext с特定ными опциями в тестах (например, для InMemory базы данных).
        public FitnessDbContext(DbContextOptions<FitnessDbContext> options) : base(options) { }
        
        // Защищенный конструктор `protected FitnessDbContext() { }` больше не нужен,
        // так как у нас есть публичный конструктор без параметров.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Этот метод вызывается, если DbContext создается БЕЗ передачи опций в конструктор
            // (т.е. когда используется `new FitnessDbContext()`).
            // Если опции переданы через конструктор (например, при DI или из тестов для InMemory),
            // то optionsBuilder.IsConfigured будет true, и этот блок не выполнится.
            if (!optionsBuilder.IsConfigured)
            {
                // Твой код для определения пути к БД - оставляем как есть
                string projectRootPathAttempt = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..")); 
                string dbFolderPath = Path.Combine(projectRootPathAttempt, "Database");
                
                // Для большей предсказуемости и если вышеописанный способ определения корня проекта
                // не всегда корректно работает (особенно при запуске тестов или EF Core команд),
                // лучше использовать более явный или надежный способ определения dbFolderPath.
                // Но твой захардкоженный путь пока оставляем для консистентности.
                dbFolderPath = @"C:\Users\lazaa\Desktop\proj1\FitnessTrainerPro\Database"; 

                if (!Directory.Exists(dbFolderPath))
                {
                    Directory.CreateDirectory(dbFolderPath);
                }
                string dbPath = Path.Combine(dbFolderPath, "fitness_trainer_abs.db"); 

                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 
            
            // Конфигурация связей (Fluent API)
            // Связь WorkoutProgram <-> ProgramExercise 
            modelBuilder.Entity<ProgramExercise>()
                .HasOne(pe => pe.WorkoutProgram)
                .WithMany(wp => wp.ProgramExercises) 
                .HasForeignKey(pe => pe.ProgramID);

            // Связь Exercise <-> ProgramExercise 
            modelBuilder.Entity<ProgramExercise>()
                .HasOne(pe => pe.Exercise)
                .WithMany() // Если у Exercise нет ICollection<ProgramExercise>
                .HasForeignKey(pe => pe.ExerciseID);

            // Связь Client <-> ClientAssignedProgram 
            modelBuilder.Entity<ClientAssignedProgram>()
                .HasOne(cap => cap.Client)
                .WithMany(c => c.AssignedPrograms) 
                .HasForeignKey(cap => cap.ClientID);
            
            // Связь WorkoutProgram <-> ClientAssignedProgram 
            modelBuilder.Entity<ClientAssignedProgram>()
                .HasOne(cap => cap.WorkoutProgram)
                .WithMany(wp => wp.ClientAssignments) 
                .HasForeignKey(cap => cap.ProgramID);

            // Связь Client <-> ClientMeasurement
            modelBuilder.Entity<ClientMeasurement>()
                .HasOne(cm => cm.Client) 
                .WithMany(c => c.Measurements) 
                .HasForeignKey(cm => cm.ClientID);
        }
    }
}