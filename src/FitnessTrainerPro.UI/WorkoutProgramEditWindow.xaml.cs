using System.Windows;
using FitnessTrainerPro.Core.Models;
using System.Collections.Generic; // Для List, хотя мы используем ObservableCollection
using System.Collections.ObjectModel; // Для ObservableCollection
using System.Linq;
using FitnessTrainerPro.Data; // Для доступа к FitnessDbContext, если понадобится

namespace FitnessTrainerPro.UI
{
    public partial class WorkoutProgramEditWindow : Window
    {
        public WorkoutProgram CurrentProgram { get; private set; }
        public ObservableCollection<ProgramExercise> ProgramExercisesList { get; set; }

        public WorkoutProgramEditWindow()
        {
            InitializeComponent();
            CurrentProgram = new WorkoutProgram();
            ProgramExercisesList = new ObservableCollection<ProgramExercise>();
            ProgramExercisesListView.ItemsSource = ProgramExercisesList;
        }

        public WorkoutProgramEditWindow(WorkoutProgram programToEdit)
        {
            InitializeComponent();
            CurrentProgram = programToEdit;
            
            ProgramNameTextBox.Text = CurrentProgram.Name;
            ProgramFocusTextBox.Text = CurrentProgram.Focus;
            ProgramDescriptionTextBox.Text = CurrentProgram.Description;

            if (CurrentProgram.ProgramExercises != null)
            {
                ProgramExercisesList = new ObservableCollection<ProgramExercise>(CurrentProgram.ProgramExercises.OrderBy(pe => pe.OrderInProgram));
            }
            else
            {
                ProgramExercisesList = new ObservableCollection<ProgramExercise>();
            }
            ProgramExercisesListView.ItemsSource = ProgramExercisesList;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentProgram.Name = ProgramNameTextBox.Text;
            CurrentProgram.Focus = ProgramFocusTextBox.Text;
            CurrentProgram.Description = ProgramDescriptionTextBox.Text;

            if (string.IsNullOrWhiteSpace(CurrentProgram.Name))
            {
                MessageBox.Show("Название программы не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Обновляем основную коллекцию в CurrentProgram
            // Важно, чтобы это была та же самая коллекция, которую EF Core отслеживает, если это редактирование
            // или чтобы она была правильно обработана при добавлении нового CurrentProgram.
            // Простой способ - очистить и добавить все заново.
            // Убедимся, что CurrentProgram.ProgramExercises инициализирован, если это новая программа.
            if (CurrentProgram.ProgramExercises == null) 
            {
                CurrentProgram.ProgramExercises = new List<ProgramExercise>();
            }
            CurrentProgram.ProgramExercises.Clear(); 
            foreach (var pe in ProgramExercisesList)
            {
                CurrentProgram.ProgramExercises.Add(pe);
            }

            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        // --- Обработчики для кнопок управления упражнениями в программе ---

        // ОСТАВЛЯЕМ ЭТОТ МЕТОД, УДАЛЯЕМ ПРЕДЫДУЩУЮ ЗАГЛУШКУ ДЛЯ НЕГО
        private void AddProgramExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            SelectExerciseWindow selectExerciseWindow = new SelectExerciseWindow();
            selectExerciseWindow.Owner = this;

            if (selectExerciseWindow.ShowDialog() == true && selectExerciseWindow.SelectedExercise != null)
            {
                Exercise chosenExercise = selectExerciseWindow.SelectedExercise;

                // TODO: Шаг Б - Открыть окно ProgramExerciseParamsWindow для ввода сетов, репов и т.д.
                // В это окно нужно будет передать chosenExercise.ExerciseID и chosenExercise.Name (для отображения)
                // и получить обратно объект ProgramExercise.

                // Пока что создадим ProgramExercise с дефолтными значениями
                ProgramExercise newProgramExercise = new ProgramExercise
                {
                    ExerciseID = chosenExercise.ExerciseID,
                    Exercise = chosenExercise, 
                    OrderInProgram = ProgramExercisesList.Count + 1, 
                    Sets = 3, 
                    Reps = "10-12", 
                    // RestSeconds = 60, // Раскомментируй, если нужно значение по умолчанию
                    // Notes = "Стандартное выполнение" // Раскомментируй, если нужно значение по умолчанию
                };

                ProgramExercisesList.Add(newProgramExercise);
                // Сообщение убрано, так как пока нет ввода параметров
                // MessageBox.Show($"Упражнение '{chosenExercise.Name}' добавлено в программу (с параметрами по умолчанию). \nРеализуйте ввод параметров.", "Упражнение добавлено");
            }
        }

        private void EditProgramExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            ProgramExercise? selectedProgramExercise = ProgramExercisesListView.SelectedItem as ProgramExercise;
            if (selectedProgramExercise == null)
            {
                MessageBox.Show("Пожалуйста, выберите упражнение из программы для редактирования.", "Внимание");
                return;
            }
            // TODO: Открыть окно для редактирования параметров выбранного ProgramExercise (ProgramExerciseParamsWindow)
            // TODO: Обновить selectedProgramExercise в ProgramExercisesList
            MessageBox.Show($"Редактирование упражнения '{selectedProgramExercise.Exercise?.Name}' еще не реализовано.", "В разработке");
        }

        private void DeleteProgramExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            ProgramExercise? selectedProgramExercise = ProgramExercisesListView.SelectedItem as ProgramExercise;
            if (selectedProgramExercise == null)
            {
                MessageBox.Show("Пожалуйста, выберите упражнение из программы для удаления.", "Внимание");
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Удалить упражнение '{selectedProgramExercise.Exercise?.Name}' из программы?",
                                                     "Подтверждение удаления",
                                                     MessageBoxButton.YesNo,
                                                     MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                ProgramExercisesList.Remove(selectedProgramExercise);
            }
        }
    }
}