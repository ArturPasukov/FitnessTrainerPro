// src/FitnessTrainerPro.Core/Services/ExerciseFilterService.cs
using FitnessTrainerPro.Core.Models;
using System.Linq;

namespace FitnessTrainerPro.Core.Services
{
    public static class ExerciseFilterService // Сделаем его статическим для простоты использования
    {
        public static IQueryable<Exercise> ApplyFilters(
            IQueryable<Exercise> query, 
            string? nameFilter, 
            string? muscleGroupFilter, 
            string? equipmentFilter)
        {
            if (!string.IsNullOrWhiteSpace(nameFilter))
            {
                string lowerFilterName = nameFilter.Trim().ToLower();
                query = query.Where(ex => ex.Name != null && ex.Name.ToLower().Contains(lowerFilterName));
            }

            if (!string.IsNullOrWhiteSpace(muscleGroupFilter))
            {
                string lowerFilterMuscleGroup = muscleGroupFilter.Trim().ToLower();
                query = query.Where(ex => ex.MuscleGroup != null && ex.MuscleGroup.ToLower().Contains(lowerFilterMuscleGroup));
            }

            if (!string.IsNullOrWhiteSpace(equipmentFilter))
            {
                string lowerFilterEquipment = equipmentFilter.Trim().ToLower();
                query = query.Where(ex => ex.EquipmentNeeded != null && ex.EquipmentNeeded.ToLower().Contains(lowerFilterEquipment));
            }

            return query;
        }
    }
}