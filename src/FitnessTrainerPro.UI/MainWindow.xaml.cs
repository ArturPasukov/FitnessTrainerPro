﻿using System.Windows;
using FitnessTrainerPro.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using FitnessTrainerPro.Core.Models;
using System.Diagnostics;
using System.Windows.Navigation;
using FitnessTrainerPro.Core.Services; // Убедись, что этот using есть

namespace FitnessTrainerPro.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadExercises(); 
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при первоначальной загрузке данных: {ex.ToString()}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ОСТАВЛЯЕМ ТОЛЬКО ЭТУ ВЕРСИЮ LoadExercises
        private void LoadExercises()
        {
            try
            {
                using (var dbContext = new FitnessDbContext())
                {
                    IQueryable<Exercise> query = dbContext.Exercises.AsQueryable();

                    // Получаем значения фильтров из TextBox'ов
                    string filterName = FilterNameTextBox.Text; 
                    string filterMuscleGroup = FilterMuscleGroupTextBox.Text;
                    string filterEquipment = FilterEquipmentTextBox.Text;

                    // Применяем фильтры через новый сервис
                    query = ExerciseFilterService.ApplyFilters(query, filterName, filterMuscleGroup, filterEquipment);

                    ExercisesListView.ItemsSource = query.OrderBy(ex => ex.Name).ToList();
                }
            }
            catch (System.Exception ex)
            {
                string errorMessage = $"Ошибка загрузки упражнений: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                }
                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // УДАЛЕНА ДУБЛИРУЮЩАЯСЯ ВЕРСИЯ LoadExercises()

        private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            LoadExercises(); 
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            FilterNameTextBox.Text = string.Empty;
            FilterMuscleGroupTextBox.Text = string.Empty;
            FilterEquipmentTextBox.Text = string.Empty;
            LoadExercises(); 
        }

        private void AddExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            ExerciseWindow exerciseWindow = new ExerciseWindow();
            exerciseWindow.Owner = this;

            if (exerciseWindow.ShowDialog() == true)
            {
                Exercise newExercise = exerciseWindow.CurrentExercise;
                try 
                {
                    using (var dbContext = new FitnessDbContext())
                    {
                        dbContext.Exercises.Add(newExercise);
                        dbContext.SaveChanges();
                    }
                    LoadExercises();
                }
                catch (System.Exception ex)
                {
                    string errorMessage = $"Ошибка добавления упражнения: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                        if (ex.InnerException.InnerException != null)
                        {
                             errorMessage += $"\n\nВложенная внутренняя ошибка: {ex.InnerException.InnerException.Message}";
                        }
                    }
                    MessageBox.Show(errorMessage, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            Exercise? selectedExercise = ExercisesListView.SelectedItem as Exercise;
            if (selectedExercise == null)
            {
                MessageBox.Show("Пожалуйста, выберите упражнение для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ExerciseWindow exerciseWindow = new ExerciseWindow(selectedExercise);
            exerciseWindow.Owner = this;

            if (exerciseWindow.ShowDialog() == true)
            {
                try
                {
                    using (var dbContext = new FitnessDbContext())
                    {
                        var exerciseInDb = dbContext.Exercises.Find(selectedExercise.ExerciseID);
                        if (exerciseInDb != null)
                        {
                            exerciseInDb.Name = exerciseWindow.CurrentExercise.Name;
                            exerciseInDb.MuscleGroup = exerciseWindow.CurrentExercise.MuscleGroup;
                            exerciseInDb.Description = exerciseWindow.CurrentExercise.Description;
                            exerciseInDb.VideoUrl = exerciseWindow.CurrentExercise.VideoUrl;
                            exerciseInDb.EquipmentNeeded = exerciseWindow.CurrentExercise.EquipmentNeeded;
                            
                            dbContext.SaveChanges();
                        }
                        else
                        {
                            MessageBox.Show("Выбранное упражнение не найдено в базе данных. Возможно, оно было удалено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    LoadExercises();
                }
                catch (DbUpdateException dbEx) 
                {
                    string errorMessage = $"Ошибка обновления упражнения в базе данных: {dbEx.Message}";
                    if (dbEx.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутренняя ошибка БД: {dbEx.InnerException.Message}";
                    }
                    MessageBox.Show(errorMessage, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.Exception ex) 
                {
                    string errorMessage = $"Общая ошибка при редактировании упражнения: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                    }
                    MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            Exercise? selectedExercise = ExercisesListView.SelectedItem as Exercise;
            if (selectedExercise == null)
            {
                MessageBox.Show("Пожалуйста, выберите упражнение для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Вы уверены, что хотите удалить упражнение '{selectedExercise.Name}'?",
                                                     "Подтверждение удаления",
                                                     MessageBoxButton.YesNo,
                                                     MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try 
                {
                    using (var dbContext = new FitnessDbContext())
                    {
                        var exerciseToDelete = dbContext.Exercises.Find(selectedExercise.ExerciseID);
                        if (exerciseToDelete != null)
                        {
                            dbContext.Exercises.Remove(exerciseToDelete);
                            dbContext.SaveChanges();
                        }
                        else
                        {
                            MessageBox.Show("Выбранное упражнение не найдено в базе данных. Возможно, оно уже было удалено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    LoadExercises();
                }
                catch (DbUpdateException dbEx)
                {
                    string errorMessage = $"Ошибка удаления упражнения из базы данных: {dbEx.Message}";
                    if (dbEx.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутренняя ошибка БД: {dbEx.InnerException.Message}";
                    }
                    MessageBox.Show(errorMessage, "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.Exception ex)
                {
                     string errorMessage = $"Общая ошибка при удалении упражнения: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                    }
                    MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (e.Uri != null && !string.IsNullOrEmpty(e.Uri.AbsoluteUri))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Не удалось открыть ссылку: {e.Uri.AbsoluteUri}\nОшибка: {ex.Message}",
                                    "Ошибка открытия ссылки", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            e.Handled = true; 
        }
        
        private void ManageClientsButton_Click(object sender, RoutedEventArgs e)
        {
            ClientManagementWindow clientManagementWindow = new ClientManagementWindow();
            clientManagementWindow.Owner = this; 
            clientManagementWindow.ShowDialog(); 
        }
        
        private void ManageProgramsButton_Click(object sender, RoutedEventArgs e)
        {
            WorkoutProgramManagementWindow programManagementWindow = new WorkoutProgramManagementWindow();
            programManagementWindow.Owner = this;
            programManagementWindow.ShowDialog();
        }

        private void OpenAnalyticsButton_Click(object sender, RoutedEventArgs e)
        {
            AnalyticsWindow analyticsWindow = new AnalyticsWindow();
            analyticsWindow.Owner = this;
            analyticsWindow.ShowDialog();
        }
        
        private void CompareProgramsButton_Click(object sender, RoutedEventArgs e)
        {
            ProgramEffectivenessWindow effectivenessWindow = new ProgramEffectivenessWindow();
            effectivenessWindow.Owner = this;
            effectivenessWindow.ShowDialog();
        }
    } 
}