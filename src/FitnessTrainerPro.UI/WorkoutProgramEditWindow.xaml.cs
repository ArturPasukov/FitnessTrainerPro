using System.Windows;
using FitnessTrainerPro.Core.Models;
using System.Collections.Generic; // Для List
using System.Collections.ObjectModel; // Для ObservableCollection
using System.Linq;
using FitnessTrainerPro.Data; // Для доступа к FitnessDbContext, если понадобится (например, для выбора упражнений)

namespace FitnessTrainerPro.UI
{
    public partial class WorkoutProgramEditWindow : Window
    {
        public WorkoutProgram CurrentProgram { get; private set; }
        // Используем ObservableCollection для автоматического обновления ListView
        public ObservableCollection<ProgramExercise> ProgramExercisesList { get; set; }

        // Конструктор для новой программы
        public WorkoutProgramEditWindow()
        {
            InitializeComponent();
            CurrentProgram = new WorkoutProgram();
            ProgramExercisesList = new ObservableCollection<ProgramExercise>();
            ProgramExercisesListView.ItemsSource = ProgramExercisesList; // Привязываем к ListView
        }

        // Конструктор для редактирования существующей программы
        public WorkoutProgramEditWindow(WorkoutProgram programToEdit)
        {
            InitializeComponent();
            CurrentProgram = programToEdit;
            
            // Заполняем основные поля программы
            ProgramNameTextBox.Text = CurrentProgram.Name;
            ProgramFocusTextBox.Text = CurrentProgram.Focus;
            ProgramDescriptionTextBox.Text = CurrentProgram.Description;

            // Заполняем список упражнений программы
            // Важно, чтобы programToEdit.ProgramExercises был загружен (через Include в вызывающем коде)
            if (CurrentProgram.ProgramExercises != null)
            {
                ProgramExercisesList = new ObservableCollection<ProgramExercise>(CurrentProgram.ProgramExercises.OrderBy(pe => pe.OrderInProgram));
            }
            else
            {
                ProgramExercisesList = new ObservableCollection<ProgramExercise>();
            }
            ProgramExercisesListView.ItemsSource = ProgramExercisesList; // Привязываем к ListView
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

            // Обновляем коллекцию ProgramExercises в CurrentProgram на основе ProgramExercisesList
            // Это важно, чтобы EF Core мог отследить изменения в коллекции
            // Сначала очистим старые, если они есть (более сложная логика синхронизации потребуется для Attach/Update)
            CurrentProgram.ProgramExercises.Clear(); 
            foreach (var pe in ProgramExercisesList)
            {
                CurrentProgram.ProgramExercises.Add(pe);
            }
            // Более продвинутый способ - синхронизировать коллекции, удаляя, обновляя и добавляя элементы.
            // Пока что просто заменяем коллекцию. Это сработает для новых программ.
            // Для редактирования существующих программ с сохранением через тот же DbContext,
            // EF Core должен отследить изменения в коллекции CurrentProgram.ProgramExercises, если она была загружена с Include.

            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        // --- Обработчики для кнопок управления упражнениями в программе ---

        private void AddProgramExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Открыть окно выбора упражнения (SelectExerciseWindow)
            // TODO: После выбора упражнения, открыть окно для ввода сетов, репов и т.д. (ProgramExerciseParamsWindow)
            // TODO: Добавить новый ProgramExercise в ProgramExercisesList
            MessageBox.Show("Функционал 'Добавить упражнение в программу' еще не реализован.", "В разработке");
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

                // Пока что создадим ProgramExercise с дефолтными значениями (или запросим их через InputBox - но это плохой UI)
                // и добавим в список.
                ProgramExercise newProgramExercise = new ProgramExercise
                {
                    ExerciseID = chosenExercise.ExerciseID,
                    Exercise = chosenExercise, // Для отображения имени в ListView сразу
                    OrderInProgram = ProgramExercisesList.Count + 1, // Простой способ задать порядок
                    Sets = 3, // Значения по умолчанию
                    Reps = "10-12", // Значения по умолчанию
                    // RestSeconds = 60,
                    // Notes = "Стандартное выполнение"
                };

                ProgramExercisesList.Add(newProgramExercise);
                MessageBox.Show($"Упражнение '{chosenExercise.Name}' добавлено в программу (с параметрами по умолчанию). \nРеализуйте ввод параметров.", "Упражнение добавлено");
            }
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
                // TODO: Подумать о том, как это отразится на CurrentProgram.ProgramExercises при сохранении.
                // Если ProgramExercisesList - это та же коллекция, что и в CurrentProgram, то удаление здесь будет достаточным
                // перед тем как EF Core сохранит изменения.
            }
        }
    }
}