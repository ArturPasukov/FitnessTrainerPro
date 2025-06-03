// FitnessDbContext.cs
using Microsoft.EntityFrameworkCore;
using FitnessTrainerPro.Core.Models;
using System.IO; 
using System;    

namespace FitnessTrainerPro.Data
{
    public class FitnessDbContext : DbContext
    {
        // Существующие DbSet'ы
        public DbSet<Client> Clients { get; set; }
        public DbSet<Exercise> Exercises { get; set; }

        // НОВЫЕ DbSet'ы
        public DbSet<WorkoutProgram> WorkoutPrograms { get; set; }
        public DbSet<ProgramExercise> ProgramExercises { get; set; }
        public DbSet<ClientAssignedProgram> ClientAssignedPrograms { get; set; }

        public FitnessDbContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Твой существующий код для определения пути к БД
                // Я его не меняю, предполагая, что он у тебя работает корректно
                string projectRootPathAttempt = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..")); 
                string dbFolderPath = Path.Combine(projectRootPathAttempt, "Database");
                
                // Использование твоего захардкоженного пути для надежности
                dbFolderPath = @"C:\Users\lazaa\Desktop\proj1\FitnessTrainerPro\Database"; 

                if (!Directory.Exists(dbFolderPath))
                {
                    Directory.CreateDirectory(dbFolderPath);
                }
                string dbPath = Path.Combine(dbFolderPath, "fitness_trainer_abs.db"); 

                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        // НОВЫЙ МЕТОД OnModelCreating
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Важно вызвать базовую реализацию

            // Связь WorkoutProgram <-> ProgramExercise (один-ко-многим)
            // У WorkoutProgram есть коллекция ProgramExercises
            modelBuilder.Entity<ProgramExercise>()
                .HasOne(pe => pe.WorkoutProgram)
                .WithMany(wp => wp.ProgramExercises) // Указываем навигационное свойство в WorkoutProgram
                .HasForeignKey(pe => pe.ProgramID);

            // Связь Exercise <-> ProgramExercise (один-ко-многим)
            // У Exercise НЕТ коллекции ProgramExercises в нашей текущей модели
            modelBuilder.Entity<ProgramExercise>()
                .HasOne(pe => pe.Exercise)
                .WithMany() // Оставляем пустым, если в Exercise нет ICollection<ProgramExercise>
                .HasForeignKey(pe => pe.ExerciseID);

            // Связь Client <-> ClientAssignedProgram (один-ко-многим)
            // У Client НЕТ коллекции ClientAssignedPrograms в нашей текущей модели
            modelBuilder.Entity<ClientAssignedProgram>()
                .HasOne(cap => cap.Client)
                .WithMany() // Оставляем пустым, если в Client нет ICollection<ClientAssignedProgram>
                .HasForeignKey(cap => cap.ClientID);
            
            // Связь WorkoutProgram <-> ClientAssignedProgram (один-ко-многим)
            // У WorkoutProgram есть коллекция ClientAssignments
            modelBuilder.Entity<ClientAssignedProgram>()
                .HasOne(cap => cap.WorkoutProgram)
                .WithMany(wp => wp.ClientAssignments) // Указываем навигационное свойство в WorkoutProgram
                .HasForeignKey(cap => cap.ProgramID);
        }
    }
}