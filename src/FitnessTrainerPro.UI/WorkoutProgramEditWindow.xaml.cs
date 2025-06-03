using System.Windows;
using FitnessTrainerPro.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;
// using FitnessTrainerPro.Data; // Этот using здесь, вероятно, не нужен напрямую, если только для сложных операций

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
                // Убедимся, что связанные Exercise загружены для отображения Exercise.Name
                // Это должно быть сделано в вызывающем коде (WorkoutProgramManagementWindow) через Include(p => p.ProgramExercises).ThenInclude(pe => pe.Exercise)
                ProgramExercisesList = new ObservableCollection<ProgramExercise>(CurrentProgram.ProgramExercises.OrderBy(pe => pe.OrderInProgram));
            }
            else
            {
                ProgramExercisesList = new ObservableCollection<ProgramExercise>();
                // Если CurrentProgram.ProgramExercises изначально null (например, для новой программы, но мы ее уже инициализируем в модели),
                // то нужно его создать, чтобы позже можно было делать Clear() и Add().
                if (CurrentProgram.ProgramExercises == null) 
                {
                    CurrentProgram.ProgramExercises = new List<ProgramExercise>();
                }
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
            
            // Убедимся, что коллекция в CurrentProgram инициализирована
            if (CurrentProgram.ProgramExercises == null)
            {
                CurrentProgram.ProgramExercises = new List<ProgramExercise>();
            }
            
            // Обновляем коллекцию ProgramExercises в CurrentProgram на основе ProgramExercisesList
            // Это важно, чтобы EF Core мог отследить изменения в коллекции.
            // Простой подход: очистить и добавить все. Позже можно оптимизировать для редактирования.
            CurrentProgram.ProgramExercises.Clear(); 
            foreach (var pe in ProgramExercisesList.OrderBy(item => item.OrderInProgram)) // Сохраняем в отсортированном порядке
            {
                // Устанавливаем ProgramID для новых ProgramExercise, если это новая программа
                // и ProgramID еще не присвоен (EF Core сделает это при добавлении WorkoutProgram)
                // Но для уже существующих ProgramExercise, ProgramID должен быть корректным.
                // Также важно, чтобы ProgramExercise.WorkoutProgram не был null, если это важно для EF Core
                // (хотя обычно FK ProgramID достаточно).
                // pe.ProgramID = CurrentProgram.ProgramID; // Обычно не нужно, если CurrentProgram.ProgramExercises - отслеживаемая коллекция
                CurrentProgram.ProgramExercises.Add(pe);
            }

            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        // --- Обработчики для кнопок управления упражнениями в программе ---

        private void AddProgramExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            SelectExerciseWindow selectExerciseWindow = new SelectExerciseWindow();
            selectExerciseWindow.Owner = this;

            if (selectExerciseWindow.ShowDialog() == true && selectExerciseWindow.SelectedExercise != null)
            {
                Exercise chosenExercise = selectExerciseWindow.SelectedExercise;

                // Предлагаем следующий порядковый номер
                int proposedOrder = ProgramExercisesList.Any() ? ProgramExercisesList.Max(pe => pe.OrderInProgram) + 1 : 1;

                ProgramExerciseParamsWindow paramsWindow = new ProgramExerciseParamsWindow(chosenExercise, proposedOrder);
                paramsWindow.Owner = this;

                if (paramsWindow.ShowDialog() == true)
                {
                    // Получаем ProgramExercise с заполненными параметрами
                    ProgramExercise newProgramExercise = paramsWindow.CurrentProgramExercise;
                    ProgramExercisesList.Add(newProgramExercise);
                    // Пересортируем список по OrderInProgram для корректного отображения
                    ResortProgramExercisesList();
                }
            }
        }

        private void EditProgramExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            ProgramExercise? selectedProgramExercise = ProgramExercisesListView.SelectedItem as ProgramExercise;
            if (selectedProgramExercise == null)
            {
                MessageBox.Show("Пожалуйста, выберите упражнение из программы для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Убедимся, что связанный Exercise загружен, если это необходимо для отображения в ParamsWindow
            // (В нашем случае ParamsWindow принимает Exercise целиком, если это новый, или ProgramExercise, если существующий)
            // Если selectedProgramExercise.Exercise равен null, нужно его подгрузить, но это должно быть сделано при загрузке ProgramExercisesList
            if (selectedProgramExercise.Exercise == null && selectedProgramExercise.ExerciseID > 0)
            {
                 // Этой ситуации быть не должно, если мы правильно загружаем данные с Include(...).ThenInclude(...)
                 MessageBox.Show("Ошибка: детали основного упражнения не загружены. Попробуйте перезагрузить программу.", "Ошибка данных");
                 return;
            }


            ProgramExerciseParamsWindow paramsWindow = new ProgramExerciseParamsWindow(selectedProgramExercise);
            paramsWindow.Owner = this;

            if (paramsWindow.ShowDialog() == true)
            {
                // Объект selectedProgramExercise уже был изменен внутри paramsWindow,
                // так как CurrentProgramExercise в paramsWindow ссылался на тот же объект.
                // ObservableCollection должна автоматически обновить UI, если свойства измененного объекта
                // вызывают PropertyChanged (если ProgramExercise реализует INotifyPropertyChanged).
                // Если нет, для простоты можно пересортировать или обновить весь список.
                ResortProgramExercisesList(); 
                ProgramExercisesListView.Items.Refresh(); // Принудительное обновление, если INotifyPropertyChanged не реализован
            }
        }

        private void DeleteProgramExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            ProgramExercise? selectedProgramExercise = ProgramExercisesListView.SelectedItem as ProgramExercise;
            if (selectedProgramExercise == null)
            {
                MessageBox.Show("Пожалуйста, выберите упражнение из программы для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Удалить упражнение '{selectedProgramExercise.Exercise?.Name}' из программы?",
                                                     "Подтверждение удаления",
                                                     MessageBoxButton.YesNo,
                                                     MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                ProgramExercisesList.Remove(selectedProgramExercise);
                // После удаления, хорошо бы пересчитать OrderInProgram для оставшихся, если это важно
                // Но для простоты пока оставим как есть.
            }
        }

        // Вспомогательный метод для пересортировки списка упражнений в UI
        private void ResortProgramExercisesList()
        {
            var sortedList = ProgramExercisesList.OrderBy(pe => pe.OrderInProgram).ToList();
            ProgramExercisesList.Clear();
            foreach (var item in sortedList)
            {
                ProgramExercisesList.Add(item);
            }
        }
    }
}