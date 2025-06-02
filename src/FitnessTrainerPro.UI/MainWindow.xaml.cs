using System.Windows;
using FitnessTrainerPro.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using FitnessTrainerPro.Core.Models;
using System.Diagnostics; // Для Process.Start
using System.Windows.Navigation; // Для RequestNavigateEventArgs

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

        // ВОТ ЭТОТ МЕТОД ДОЛЖЕН БЫТЬ В КЛАССЕ MainWindow
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
            e.Handled = true; // Помечаем, что мы обработали событие навигации
        }
    }
}