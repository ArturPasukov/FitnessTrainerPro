// src/FitnessTrainerPro.Core/Services/ISimpleExerciseDbService.cs
using FitnessTrainerPro.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitnessTrainerPro.Core.Services
{
    public interface ISimpleExerciseDbService
    {
        Task AddAsync(Exercise exercise);
        Task<Exercise?> GetByIdAsync(int id);
        Task UpdateAsync(Exercise exercise);
        Task DeleteAsync(int id);
        Task<List<Exercise>> GetAllAsync();
    }
}