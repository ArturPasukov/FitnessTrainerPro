// FitnessTrainerPro.Data/Services/SimpleExerciseDbService.cs (ПРИМЕР)
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic; // Для List
using System.Linq; // Для OrderBy


public interface ISimpleExerciseDbService
{
    Task AddAsync(Exercise exercise);
    Task<Exercise?> GetByIdAsync(int id);
    Task UpdateAsync(Exercise exercise);
    Task DeleteAsync(int id);
    Task<List<Exercise>> GetAllAsync(); // Для простоты без фильтров здесь
}

public class SimpleExerciseDbService : ISimpleExerciseDbService
{
    private readonly FitnessDbContext _context;

    public SimpleExerciseDbService(FitnessDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Exercise exercise)
    {
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
        var existing = await _context.Exercises.FindAsync(exercise.ExerciseID);
        if (existing != null)
        {
            _context.Entry(existing).CurrentValues.SetValues(exercise);
            await _context.SaveChangesAsync();
        }
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