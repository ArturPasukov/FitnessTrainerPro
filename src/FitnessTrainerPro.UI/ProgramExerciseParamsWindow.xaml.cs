using System.Windows;
using FitnessTrainerPro.Core.Models;
using System; // Для Convert

namespace FitnessTrainerPro.UI
{
    public partial class ProgramExerciseParamsWindow : Window
    {
        public ProgramExercise CurrentProgramExercise { get; private set; }

        // Конструктор для нового ProgramExercise (когда мы выбрали Exercise и теперь вводим его параметры)
        public ProgramExerciseParamsWindow(Exercise selectedExercise, int proposedOrder)
        {
            InitializeComponent();
            CurrentProgramExercise = new ProgramExercise
            {
                ExerciseID = selectedExercise.ExerciseID,
                Exercise = selectedExercise, // Сохраняем ссылку для информации
                OrderInProgram = proposedOrder
                // Сеты, репы и т.д. будут введены пользователем
            };
            ExerciseNameTextBlock.Text = selectedExercise.Name; // Отображаем название упражнения
            OrderInProgramTextBox.Text = proposedOrder.ToString();
        }

        // Конструктор для редактирования существующего ProgramExercise
        public ProgramExerciseParamsWindow(ProgramExercise programExerciseToEdit)
        {
            InitializeComponent();
            CurrentProgramExercise = programExerciseToEdit;

            ExerciseNameTextBlock.Text = programExerciseToEdit.Exercise?.Name ?? "Упражнение не указано"; // Отображаем название
            OrderInProgramTextBox.Text = CurrentProgramExercise.OrderInProgram.ToString();
            SetsTextBox.Text = CurrentProgramExercise.Sets.ToString();
            RepsTextBox.Text = CurrentProgramExercise.Reps;
            RestSecondsTextBox.Text = CurrentProgramExercise.RestSeconds?.ToString(); // '?' так как RestSeconds nullable int
            NotesTextBox.Text = CurrentProgramExercise.Notes;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация и считывание данных
            if (!int.TryParse(OrderInProgramTextBox.Text, out int order) || order <= 0)
            {
                MessageBox.Show("Порядок должен быть положительным числом.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!int.TryParse(SetsTextBox.Text, out int sets) || sets <= 0)
            {
                MessageBox.Show("Количество подходов (Сеты) должно быть положительным числом.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(RepsTextBox.Text))
            {
                MessageBox.Show("Количество повторений (Репы) не может быть пустым.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int? restSeconds = null;
            if (!string.IsNullOrWhiteSpace(RestSecondsTextBox.Text))
            {
                if (int.TryParse(RestSecondsTextBox.Text, out int rs) && rs >= 0)
                {
                    restSeconds = rs;
                }
                else
                {
                    MessageBox.Show("Время отдыха должно быть неотрицательным числом (или пустым).", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Обновляем объект CurrentProgramExercise
            CurrentProgramExercise.OrderInProgram = order;
            CurrentProgramExercise.Sets = sets;
            CurrentProgramExercise.Reps = RepsTextBox.Text;
            CurrentProgramExercise.RestSeconds = restSeconds;
            CurrentProgramExercise.Notes = NotesTextBox.Text;
            // ExerciseID и Exercise уже должны быть установлены в конструкторе

            this.DialogResult = true; // Закрываем окно с результатом OK
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Закрываем окно с результатом Отмена
        }
    }
}