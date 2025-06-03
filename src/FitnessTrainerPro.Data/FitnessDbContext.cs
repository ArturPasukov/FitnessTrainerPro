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
        public DbSet<WorkoutProgram> WorkoutPrograms { get; set; }
        public DbSet<ProgramExercise> ProgramExercises { get; set; }
        public DbSet<ClientAssignedProgram> ClientAssignedPrograms { get; set; }

        // НОВЫЙ DbSet для замеров
        public DbSet<ClientMeasurement> ClientMeasurements { get; set; }


        public FitnessDbContext() { }

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

            // НОВАЯ КОНФИГУРАЦИЯ СВЯЗИ Client <-> ClientMeasurement
            modelBuilder.Entity<ClientMeasurement>()
                .HasOne(cm => cm.Client) // У ClientMeasurement есть свойство Client
                .WithMany(c => c.Measurements) // У Client есть коллекция Measurements
                .HasForeignKey(cm => cm.ClientID); // Внешний ключ в ClientMeasurement
        }
    }
}