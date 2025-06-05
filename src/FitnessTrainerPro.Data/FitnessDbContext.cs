// FitnessDbContext.cs
using Microsoft.EntityFrameworkCore;
using FitnessTrainerPro.Core.Models;
using System.IO; 
using System;    

namespace FitnessTrainerPro.Data
{
    public class FitnessDbContext : DbContext
    {
        // ИЗМЕНЕНО: Добавлено 'virtual'
        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<Exercise> Exercises { get; set; }
        public virtual DbSet<WorkoutProgram> WorkoutPrograms { get; set; }
        public virtual DbSet<ProgramExercise> ProgramExercises { get; set; }
        public virtual DbSet<ClientAssignedProgram> ClientAssignedPrograms { get; set; }
        public virtual DbSet<ClientMeasurement> ClientMeasurements { get; set; }


        // Конструктор для Moq (и для DI, если будешь использовать)
        // Если EF Core Tools жалуются на отсутствие конструктора без параметров при создании миграций,
        // можно его оставить, но для моков лучше передавать options.
        // public FitnessDbContext() { } 

        // Этот конструктор нужен, чтобы Moq мог создать экземпляр,
        // или если ты будешь использовать DI и передавать DbContextOptions.
        // Если у тебя уже есть конструктор по умолчанию public FitnessDbContext() {},
        // то для Moq можно его использовать, но для реального приложения лучше с options.
        // Для миграций EF Core нужен либо конструктор без параметров, либо такой, который может быть вызван DI.
        // Оставим твой OnConfiguring, но для моков этот конструктор может быть полезен,
        // если бы мы не использовали мок самого FitnessDbContext, а реальный с InMemory-провайдером.
        // Но так как мы мокаем FitnessDbContext целиком, наличие этого конструктора не так критично для тестов.
        // Однако, если ты УЖЕ используешь конструктор по умолчанию для EF Core миграций,
        // то оставь его, и Moq его подхватит.
        
        // Если у тебя есть конструктор public FitnessDbContext() {}, то Moq сможет его использовать.
        // Если нет, и EF Core не жалуется на миграции, то все ок.
        // Для полной уверенности, что Moq может создать экземпляр, можно добавить:
        public FitnessDbContext(DbContextOptions<FitnessDbContext> options) : base(options) { }
        protected FitnessDbContext() { } // Защищенный конструктор для Moq, если DbContextOptions не нужны для мока

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string projectRootPathAttempt = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..")); 
                string dbFolderPath = Path.Combine(projectRootPathAttempt, "Database");
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
            // ... (твои существующие конфигурации связей остаются без изменений) ...
            // Связь WorkoutProgram <-> ProgramExercise 
            modelBuilder.Entity<ProgramExercise>()
                .HasOne(pe => pe.WorkoutProgram)
                .WithMany(wp => wp.ProgramExercises) 
                .HasForeignKey(pe => pe.ProgramID);

            // Связь Exercise <-> ProgramExercise 
            modelBuilder.Entity<ProgramExercise>()
                .HasOne(pe => pe.Exercise)
                .WithMany() 
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