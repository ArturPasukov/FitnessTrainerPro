using System.Windows;
using FitnessTrainerPro.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using FitnessTrainerPro.Core.Models; // Остается нужным

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
                MessageBox.Show($"Произошла ошибка при загрузке данных: {ex.ToString()}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadExercises()
        {
            using (var dbContext = new FitnessDbContext())
            {
                ExercisesListView.ItemsSource = dbContext.Exercises.ToList();
            }
        }

        private void AddExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            ExerciseWindow exerciseWindow = new ExerciseWindow();
            exerciseWindow.Owner = this;

            if (exerciseWindow.ShowDialog() == true)
            {
                Exercise newExercise = exerciseWindow.CurrentExercise;
                using (var dbContext = new FitnessDbContext())
                {
                    dbContext.Exercises.Add(newExercise);
                    dbContext.SaveChanges();
                }
                LoadExercises();
            }
        }

        private void EditExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранное упражнение из ListView
            // Добавляем '?' для указания, что selectedExercise может быть null
            Exercise? selectedExercise = ExercisesListView.SelectedItem as Exercise; 
            if (selectedExercise == null)
            {
                MessageBox.Show("Пожалуйста, выберите упражнение для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Exercise exerciseToEdit = selectedExercise; // Эта строка не обязательна, т.к. selectedExercise уже не null
                                                       // и мы его передаем напрямую

            ExerciseWindow exerciseWindow = new ExerciseWindow(selectedExercise); // Передаем упражнение в окно
            exerciseWindow.Owner = this;

            if (exerciseWindow.ShowDialog() == true)
            {
                // CurrentExercise в exerciseWindow это тот же объект selectedExercise, который был изменен
                using (var dbContext = new FitnessDbContext())
                {
                    // Так как exerciseWindow.CurrentExercise - это тот же объект, что и selectedExercise,
                    // который был получен из предыдущего контекста (неявно, через ListView),
                    // EF Core может не отследить его изменения, если контекст создается заново.
                    // Поэтому лучше найти объект в текущем контексте и обновить его свойства.
                    var exerciseInDb = dbContext.Exercises.Find(selectedExercise.ExerciseID);
                    if (exerciseInDb != null)
                    {
                        exerciseInDb.Name = exerciseWindow.CurrentExercise.Name; 
                        exerciseInDb.MuscleGroup = exerciseWindow.CurrentExercise.MuscleGroup;
                        exerciseInDb.Description = exerciseWindow.CurrentExercise.Description;
                        // Добавить другие поля если есть (например, VideoUrl, EquipmentNeeded позже)
                        dbContext.SaveChanges();
                    }
                    else
                    {
                        // Обработка случая, если упражнение было удалено другим пользователем/процессом
                        MessageBox.Show("Выбранное упражнение не найдено в базе данных. Возможно, оно было удалено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                LoadExercises();
            }
        }

        private void DeleteExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            // Добавляем '?' для указания, что selectedExercise может быть null
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
        }
    }
}