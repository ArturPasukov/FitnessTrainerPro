using System.Windows;
using FitnessTrainerPro.Data; // Для доступа к FitnessDbContext
using Microsoft.EntityFrameworkCore; // Для ToListAsync() или ToList()
using System.Linq; // Для ToList()
// Если будешь добавлять тестовые данные, понадобится:
// using FitnessTrainerPro.Core.Models;

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
            // ВРЕМЕННО: Добавим тестовые данные, если база пуста
            // Закомментируй или удали этот блок после первой успешной проверки
            using (var dbContext = new FitnessDbContext())
            {
                if (!dbContext.Exercises.Any()) // Проверяем, есть ли уже упражнения
                {
                    dbContext.Exercises.Add(new Core.Models.Exercise { Name = "Приседания", MuscleGroup = "Ноги", Description = "Базовое упражнение" });
                    dbContext.Exercises.Add(new Core.Models.Exercise { Name = "Жим лежа", MuscleGroup = "Грудь", Description = "Для грудных мышц" });
                    dbContext.SaveChanges();
                }
            }
            // Конец временного блока

            LoadExercises();
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