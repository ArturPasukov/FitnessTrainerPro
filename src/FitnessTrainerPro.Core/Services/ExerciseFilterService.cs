// src/FitnessTrainerPro.Core/Services/ExerciseFilterService.cs
using FitnessTrainerPro.Core.Models;
using System.Linq;
// using System.Diagnostics; // Больше не нужен здесь для этого метода

namespace FitnessTrainerPro.Core.Services
{
    public static class ExerciseFilterService 
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
                // System.Diagnostics.Debug.WriteLine($"[ExerciseFilterService] Фильтр по имени применяется: '{lowerFilterName}'"); // Можно оставить снаружи
                query = query.Where(ex => ex.Name != null && ex.Name.ToLower().Contains(lowerFilterName));
            }

            if (!string.IsNullOrWhiteSpace(muscleGroupFilter))
            {
                string lowerFilterMuscleGroup = muscleGroupFilter.Trim().ToLower();
                // System.Diagnostics.Debug.WriteLine($"[ExerciseFilterService] Фильтр по группе мышц применяется: '{lowerFilterMuscleGroup}'");
                query = query.Where(ex => ex.MuscleGroup != null && ex.MuscleGroup.ToLower().Contains(lowerFilterMuscleGroup));
            }

            if (!string.IsNullOrWhiteSpace(equipmentFilter))
            {
                string lowerFilterEquipment = equipmentFilter.Trim().ToLower();
                // System.Diagnostics.Debug.WriteLine($"[ExerciseFilterService] Фильтр по инвентарю применяется: '{lowerFilterEquipment}'");
                query = query.Where(ex => ex.EquipmentNeeded != null && ex.EquipmentNeeded.ToLower().Contains(lowerFilterEquipment));
            }

            return query;
        }
    }
}