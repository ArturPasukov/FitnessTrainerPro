using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Core.ViewModels; // Предполагаем, что ViewModel здесь
using FitnessTrainerPro.Data;
using Microsoft.EntityFrameworkCore;


namespace FitnessTrainerPro.UI
{
    public partial class ProgramEffectivenessWindow : Window
    {
        public ProgramEffectivenessWindow()
        {
            InitializeComponent();
            // Loaded="Window_Loaded" должен быть в XAML
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
                    // Загружаем программы со всеми необходимыми связанными данными
                    var programs = dbContext.WorkoutPrograms
                                       .Include(wp => wp.ClientAssignments)
                                           .ThenInclude(ca => ca.Client) // Убедимся, что Client не null
                                               .ThenInclude(c => c!.Measurements) // Используем '!', если уверены что Client не null после Include
                                       .ToList();

                    foreach (var program in programs)
                    {
                        var viewModel = new ProgramEffectivenessViewModel
                        {
                            ProgramName = program.Name ?? "Без названия"
                        };

                        List<double> weightChangesKg = new List<double>();
                        List<double> percentageChanges = new List<double>();

                        if (program.ClientAssignments != null)
                        {
                            foreach (var assignment in program.ClientAssignments)
                            {
                                // Проверяем Client и Measurements на null перед использованием
                                if (assignment.Client == null || assignment.Client.Measurements == null || !assignment.Client.Measurements.Any())
                                    continue;

                                DateTime assignmentEndDate = assignment.EndDate ?? DateTime.Now;

                                var measurementsDuringProgram = assignment.Client.Measurements
                                    .Where(m => m.MeasurementDate >= assignment.StartDate && 
                                                m.MeasurementDate <= assignmentEndDate && 
                                                m.WeightKg.HasValue)
                                    .OrderBy(m => m.MeasurementDate)
                                    .ToList();

                                if (measurementsDuringProgram.Count >= 2)
                                {
                                    // Мы уже проверили WeightKg.HasValue в Where, поэтому !.Value безопасно
                                    double startWeight = measurementsDuringProgram.First().WeightKg!.Value; 
                                    double endWeight = measurementsDuringProgram.Last().WeightKg!.Value;
                                    
                                    weightChangesKg.Add(endWeight - startWeight);

                                    if (startWeight != 0)
                                    {
                                        percentageChanges.Add(((endWeight - startWeight) / startWeight) * 100);
                                    }
                                }
                            }
                        }
                        
                        viewModel.ClientCount = weightChangesKg.Count; 
                        if (weightChangesKg.Any())
                        {
                            viewModel.AvgWeightChangeKg = weightChangesKg.Average();
                        }
                        else
                        {
                            viewModel.AvgWeightChangeKg = 0;
                        }

                        if(percentageChanges.Any())
                        {
                            viewModel.AvgWeightChangePercent = percentageChanges.Average();
                        }
                        else
                        {
                             viewModel.AvgWeightChangePercent = 0;
                        }
                        effectivenessData.Add(viewModel);
                    }
                }
                // Предполагается, что EffectivenessListView - это имя ListView в ProgramEffectivenessWindow.xaml
                EffectivenessListView.ItemsSource = effectivenessData.OrderByDescending(vm => vm.AvgWeightChangePercent).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных об эффективности программ: {ex.Message}\n{ex.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}