using System.Windows;
using FitnessTrainerPro.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using FitnessTrainerPro.Core.Models; // Остается нужным
// using System.Collections.Generic; // Может понадобиться для обновления списка

namespace FitnessTrainerPro.UI
{
    public partial class MainWindow : Window
    {
        // Убери временный код добавления данных, если еще не убрал
        // ... (конструктор и Window_Loaded без временного блока) ...

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
                MessageBox.Show($"Произошла ошибка при загрузке данных: {ex.ToString()}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadExercises()
        {
            using (var dbContext = new FitnessDbContext())
            {
                // Загружаем упражнения и сразу присваиваем источнику данных ListView
                // Чтобы избежать проблем с отслеживанием изменений, лучше каждый раз получать новый список
                ExercisesListView.ItemsSource = dbContext.Exercises.ToList();
            }
        }

        private void AddExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            ExerciseWindow exerciseWindow = new ExerciseWindow(); // Создаем окно для нового упражнения
            exerciseWindow.Owner = this; // Чтобы окно было модальным относительно главного

            if (exerciseWindow.ShowDialog() == true) // Показываем окно и ждем результата
            {
                // Если пользователь нажал OK
                Exercise newExercise = exerciseWindow.CurrentExercise;
                using (var dbContext = new FitnessDbContext())
                {
                    dbContext.Exercises.Add(newExercise);
                    dbContext.SaveChanges();
                }
                LoadExercises(); // Обновляем список в ListView
            }
        }

        private void EditExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранное упражнение из ListView
            Exercise selectedExercise = ExercisesListView.SelectedItem as Exercise;
            if (selectedExercise == null)
            {
                MessageBox.Show("Пожалуйста, выберите упражнение для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Создаем копию для редактирования, чтобы изменения не затрагивали оригинал до сохранения
            // Хотя в нашем простом случае ExerciseWindow работает с переданным объектом напрямую
            Exercise exerciseToEdit = selectedExercise; 

            ExerciseWindow exerciseWindow = new ExerciseWindow(exerciseToEdit); // Передаем упражнение в окно
            exerciseWindow.Owner = this;

            if (exerciseWindow.ShowDialog() == true)
            {
                // Пользователь нажал OK, CurrentExercise в exerciseWindow уже обновлен
                using (var dbContext = new FitnessDbContext())
                {
                    // EF Core отследит изменения в selectedExercise, если он был получен из этого же контекста
                    // Но для надежности, особенно если контекст пересоздается, лучше так:
                    var exerciseInDb = dbContext.Exercises.Find(selectedExercise.ExerciseID);
                    if (exerciseInDb != null)
                    {
                        exerciseInDb.Name = exerciseWindow.CurrentExercise.Name; // exerciseWindow.CurrentExercise это тот же selectedExercise
                        exerciseInDb.MuscleGroup = exerciseWindow.CurrentExercise.MuscleGroup;
                        exerciseInDb.Description = exerciseWindow.CurrentExercise.Description;
                        // Добавить другие поля если есть
                        dbContext.SaveChanges();
                    }
                }
                LoadExercises(); // Обновляем список
            }
        }

        private void DeleteExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            Exercise selectedExercise = ExercisesListView.SelectedItem as Exercise;
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
                using (var dbContext = new FitnessDbContext())
                {
                    // Находим упражнение в базе и удаляем
                    var exerciseToDelete = dbContext.Exercises.Find(selectedExercise.ExerciseID);
                    if (exerciseToDelete != null)
                    {
                        dbContext.Exercises.Remove(exerciseToDelete);
                        dbContext.SaveChanges();
                    }
                }
                LoadExercises(); // Обновляем список
            }
        }
    }
}