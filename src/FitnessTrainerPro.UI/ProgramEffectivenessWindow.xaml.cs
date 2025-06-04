using System.Collections.Generic;
using System.Linq;
using System.Windows;
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Core.ViewModels; // Если ViewModel в Core
// using FitnessTrainerPro.UI.ViewModels; // Если ViewModel в UI
using FitnessTrainerPro.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace FitnessTrainerPro.UI
{
    public partial class ProgramEffectivenessWindow : Window
    {
        public ProgramEffectivenessWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEffectivenessData();
        }

        private void LoadEffectivenessData()
        {
            List<ProgramEffectivenessViewModel> effectivenessData = new List<ProgramEffectivenessViewModel>();

            try
            {
                using (var dbContext = new FitnessDbContext())
                {
                    var programs = dbContext.WorkoutPrograms
                                       .Include(wp => wp.ClientAssignments) // Включаем назначения этой программы
                                           .ThenInclude(ca => ca.Client)       // Включаем клиента в назначении
                                               .ThenInclude(c => c.Measurements) // И замеры этого клиента
                                       .ToList();

                    foreach (var program in programs)
                    {
                        var viewModel = new ProgramEffectivenessViewModel
                        {
                            ProgramName = program.Name ?? "Без названия"
                        };

                        List<double> weightChangesKg = new List<double>();

                        if (program.ClientAssignments != null)
                        {
                            foreach (var assignment in program.ClientAssignments)
                            {
                                if (assignment.Client?.Measurements == null || !assignment.Client.Measurements.Any())
                                    continue;

                                // Ищем замеры в рамках дат назначения программы
                                // Если EndDate null, то берем все замеры после StartDate до текущего момента или последнего замера
                                DateTime assignmentEndDate = assignment.EndDate ?? DateTime.Now;

                                var measurementsDuringProgram = assignment.Client.Measurements
                                    .Where(m => m.MeasurementDate >= assignment.StartDate && 
                                                m.MeasurementDate <= assignmentEndDate && 
                                                m.WeightKg.HasValue)
                                    .OrderBy(m => m.MeasurementDate)
                                    .ToList();

                                if (measurementsDuringProgram.Count >= 2) // Нужно хотя бы 2 замера для расчета изменения
                                {
                                    double startWeight = measurementsDuringProgram.First().WeightKg!.Value;
                                    double endWeight = measurementsDuringProgram.Last().WeightKg!.Value;
                                    
                                    weightChangesKg.Add(endWeight - startWeight);
                                }
                            }
                        }
                        
                        viewModel.ClientCount = weightChangesKg.Count; // Количество клиентов, для которых удалось посчитать изменение
                        if (weightChangesKg.Any())
                        {
                            viewModel.AvgWeightChangeKg = weightChangesKg.Average();
                            // Для процента нужно быть осторожнее с делением на ноль и разными начальными весами.
                            // Это упрощенный расчет среднего процента, более точный был бы сложнее.
                            // Мы можем посчитать средний процент изменения для каждого клиента, а потом усреднить проценты.
                            // Или, как здесь, среднее абсолютное изменение, а потом процент от среднего начального веса (что не совсем корректно).
                            // Давайте пока оставим так для простоты, или просто покажем абсолютное изменение.
                            // Для корректного среднего процента:
                            List<double> percentageChanges = new List<double>();
                            foreach (var assignment in program.ClientAssignments)
                            {
                                // ... (повторный поиск замеров, как выше) ...
                                 var measurementsDuringProgram = assignment.Client?.Measurements?
                                    .Where(m => m.MeasurementDate >= assignment.StartDate && 
                                                m.MeasurementDate <= (assignment.EndDate ?? DateTime.Now) && 
                                                m.WeightKg.HasValue)
                                    .OrderBy(m => m.MeasurementDate)
                                    .ToList();
                                if (measurementsDuringProgram != null && measurementsDuringProgram.Count >= 2)
                                {
                                    double startW = measurementsDuringProgram.First().WeightKg!.Value;
                                    double endW = measurementsDuringProgram.Last().WeightKg!.Value;
                                    if (startW != 0)
                                    {
                                        percentageChanges.Add(((endW - startW) / startW) * 100);
                                    }
                                }
                            }
                            if(percentageChanges.Any())
                                viewModel.AvgWeightChangePercent = percentageChanges.Average();
                        }
                        else
                        {
                            viewModel.AvgWeightChangeKg = 0;
                            viewModel.AvgWeightChangePercent = 0;
                        }
                        effectivenessData.Add(viewModel);
                    }
                }
                EffectivenessListView.ItemsSource = effectivenessData.OrderByDescending(vm => vm.AvgWeightChangePercent).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных об эффективности программ: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}