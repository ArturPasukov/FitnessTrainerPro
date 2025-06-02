using System.Windows;
using FitnessTrainerPro.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using FitnessTrainerPro.Core.Models; // <--- Этот using очень важен!

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
                // ВРЕМЕННО: Добавим тестовые данные, если база пуста
                using (var dbContext = new FitnessDbContext())
                {
                    if (!dbContext.Exercises.Any())
                    {
                        // Убедись, что Exercise - это FitnessTrainerPro.Core.Models.Exercise
                        dbContext.Exercises.Add(new Exercise { Name = "Приседания", MuscleGroup = "Ноги", Description = "Базовое упражнение" });
                        dbContext.Exercises.Add(new Exercise { Name = "Жим лежа", MuscleGroup = "Грудь", Description = "Для грудных мышц" });
                        dbContext.SaveChanges();
                    }
                }
                // Конец временного блока

                LoadExercises();
            }
            catch (System.Exception ex) 
            {
                MessageBox.Show($"Произошла ошибка при загрузке данных: {ex.ToString()}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadExercises()
        {
            using (var dbContext = new FitnessDbContext())
            {
                var exercises = dbContext.Exercises.ToList();
                ExercisesListView.ItemsSource = exercises;
            }
        }
    }
}