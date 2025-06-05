// src/FitnessTrainerPro.Data/Services/SimpleExerciseDbService.cs
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Core.Services; // <--- ДОБАВЬ ЭТОТ USING
using FitnessTrainerPro.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System; // Для ArgumentNullException

namespace FitnessTrainerPro.Data.Services // Убедись, что namespace правильный
{
    public class SimpleExerciseDbService : ISimpleExerciseDbService // <--- ИЗМЕНЕНО: реализуем интерфейс
    {
        private readonly FitnessDbContext _context;

        public SimpleExerciseDbService(FitnessDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(Exercise exercise)
        {
            if (exercise == null) throw new ArgumentNullException(nameof(exercise));
            await _context.Exercises.AddAsync(exercise);
            await _context.SaveChangesAsync();
        }

        public async Task<Exercise?> GetByIdAsync(int id)
        {
            return await _context.Exercises.FindAsync(id);
        }
        public async Task<List<Exercise>> GetAllAsync()
        {
            return await _context.Exercises.OrderBy(e => e.Name).ToListAsync();
        }

        public async Task UpdateAsync(Exercise exercise)
        {
            if (exercise == null) throw new ArgumentNullException(nameof(exercise));
            var existing = await _context.Exercises.FindAsync(exercise.ExerciseID);
            if (existing != null)
            {
                existing.Name = exercise.Name;
                existing.Description = exercise.Description;
                existing.MuscleGroup = exercise.MuscleGroup;
                existing.VideoUrl = exercise.VideoUrl;
                existing.EquipmentNeeded = exercise.EquipmentNeeded;
                await _context.SaveChangesAsync();
            }
            // Можно добавить else и логирование/исключение, если existing == null
        }

        public async Task DeleteAsync(int id)
        {
            var exercise = await _context.Exercises.FindAsync(id);
            if (exercise != null)
            {
                _context.Exercises.Remove(exercise);
                await _context.SaveChangesAsync();
            }
        }
    }
}