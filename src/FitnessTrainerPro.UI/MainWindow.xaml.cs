using System.Windows;
using FitnessTrainerPro.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using FitnessTrainerPro.Core.Models;
using System.Diagnostics;
using System.Windows.Navigation;
// using System.Windows.Controls; // Может понадобиться, если будешь использовать TextChanged для авто-фильтрации

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
                LoadExercises(); // Загружаем все упражнения при старте (фильтры будут пустыми)
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при первоначальной загрузке данных: {ex.ToString()}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ИЗМЕНЕННЫЙ МЕТОД ДЛЯ ЗАГРУЗКИ УПРАЖНЕНИЙ С УЧЕТОМ ФИЛЬТРОВ
                private void LoadExercises()
        {
            try
            {
                using (var dbContext = new FitnessDbContext())
                {
                    IQueryable<Exercise> query = dbContext.Exercises.AsQueryable();

                    string filterName = FilterNameTextBox.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(filterName))
                    {
                        // ИЗМЕНЕНО: ToLowerInvariant() на ToLower()
                        string lowerFilterName = filterName.ToLower(); // Приводим к нижнему регистру один раз
                        query = query.Where(ex => ex.Name != null && ex.Name.ToLower().Contains(lowerFilterName));
                    }

                    string filterMuscleGroup = FilterMuscleGroupTextBox.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(filterMuscleGroup))
                    {
                        // ИЗМЕНЕНО: ToLowerInvariant() на ToLower()
                        string lowerFilterMuscleGroup = filterMuscleGroup.ToLower();
                        query = query.Where(ex => ex.MuscleGroup != null && ex.MuscleGroup.ToLower().Contains(lowerFilterMuscleGroup));
                    }

                    string filterEquipment = FilterEquipmentTextBox.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(filterEquipment))
                    {
                        // ИЗМЕНЕНО: ToLowerInvariant() на ToLower()
                        string lowerFilterEquipment = filterEquipment.ToLower();
                        query = query.Where(ex => ex.EquipmentNeeded != null && ex.EquipmentNeeded.ToLower().Contains(lowerFilterEquipment));
                    }

                    ExercisesListView.ItemsSource = query.OrderBy(ex => ex.Name).ToList();
                }
            }
            catch (System.Exception ex)
            {
                // Выведем и InnerException, если он есть, для лучшей диагностики
                string errorMessage = $"Ошибка загрузки упражнений: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                }
                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // НОВЫЙ ОБРАБОТЧИК ДЛЯ КНОПКИ "ПРИМЕНИТЬ ФИЛЬТР"
        private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            LoadExercises(); 
        }

        // НОВЫЙ ОБРАБОТЧИК ДЛЯ КНОПКИ "СБРОСИТЬ ФИЛЬТРЫ"
        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            FilterNameTextBox.Text = string.Empty;
            FilterMuscleGroupTextBox.Text = string.Empty;
            FilterEquipmentTextBox.Text = string.Empty;
            LoadExercises(); 
        }

        // --- Существующие обработчики ---
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
                    LoadExercises(); // Важно вызывать LoadExercises, чтобы применились фильтры
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
                    LoadExercises(); // Важно вызывать LoadExercises
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
                    LoadExercises(); // Важно вызывать LoadExercises
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
    } 
}