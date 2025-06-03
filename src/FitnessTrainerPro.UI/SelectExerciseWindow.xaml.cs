using System.Windows;
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Data;
using System.Linq;
using System.Collections.Generic; // Для List
using System.Windows.Input; // Для MouseButtonEventArgs

namespace FitnessTrainerPro.UI
{
    public partial class SelectExerciseWindow : Window
    {
        public Exercise? SelectedExercise { get; private set; } // Свойство для возврата выбранного упражнения

        public SelectExerciseWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAllExercises();
        }

        private void LoadAllExercises()
        {
            try
            {
                using (var dbContext = new FitnessDbContext())
                {
                    ExercisesListView.ItemsSource = dbContext.Exercises.OrderBy(ex => ex.Name).ToList();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка упражнений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmSelection();
        }

        private void ExercisesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ConfirmSelection();
        }

        private void ConfirmSelection()
        {
            if (ExercisesListView.SelectedItem is Exercise exercise)
            {
                SelectedExercise = exercise;
                this.DialogResult = true; // Закрываем окно с результатом OK
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите упражнение.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Закрываем окно с результатом Отмена
        }
    }
}