using System.Windows;
using FitnessTrainerPro.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using FitnessTrainerPro.Core.Models; // Этот using остается, т.к. Exercise - это модель

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
                // ---- НАЧАЛО БЛОКА, КОТОРЫЙ НУЖНО УДАЛИТЬ ИЛИ ЗАКОММЕНТИРОВАТЬ ----
                /* // Можно закомментировать так
                // ВРЕМЕННО: Добавим тестовые данные, если база пуста
                using (var dbContext = new FitnessDbContext())
                {
                    if (!dbContext.Exercises.Any())
                    {
                        dbContext.Exercises.Add(new Exercise { Name = "Приседания", MuscleGroup = "Ноги", Description = "Базовое упражнение" });
                        dbContext.Exercises.Add(new Exercise { Name = "Жим лежа", MuscleGroup = "Грудь", Description = "Для грудных мышц" });
                        dbContext.SaveChanges();
                    }
                }
                */ // Конец комментирования
                // ---- КОНЕЦ БЛОКА, КОТОРЫЙ НУЖНО УДАЛИТЬ ИЛИ ЗАКОММЕНТИРОВАТЬ ----

                LoadExercises(); // Эта строка остается - она загружает данные для отображения
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
                var exercises = dbContext.Exercises.ToList(); // Загружаем все упражнения
                ExercisesListView.ItemsSource = exercises;    // Отображаем их в списке
            }
        }
    }
}