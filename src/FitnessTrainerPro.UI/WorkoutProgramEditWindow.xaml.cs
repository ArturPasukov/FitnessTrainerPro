using System.Windows;
using FitnessTrainerPro.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;
// using FitnessTrainerPro.Data; // Этот using здесь, вероятно, не нужен напрямую

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
            
            if (CurrentProgram.ProgramExercises == null)
            {
                CurrentProgram.ProgramExercises = new List<ProgramExercise>();
            }
            
            CurrentProgram.ProgramExercises.Clear(); 
            foreach (var pe in ProgramExercisesList.OrderBy(item => item.OrderInProgram))
            {
                CurrentProgram.ProgramExercises.Add(pe);
            }

            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void AddProgramExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            SelectExerciseWindow selectExerciseWindow = new SelectExerciseWindow();
            selectExerciseWindow.Owner = this;

            if (selectExerciseWindow.ShowDialog() == true && selectExerciseWindow.SelectedExercise != null)
            {
                Exercise chosenExercise = selectExerciseWindow.SelectedExercise;
                int proposedOrder = ProgramExercisesList.Any() ? ProgramExercisesList.Max(pe => pe.OrderInProgram) + 1 : 1;

                ProgramExerciseParamsWindow paramsWindow = new ProgramExerciseParamsWindow(chosenExercise, proposedOrder);
                paramsWindow.Owner = this;

                if (paramsWindow.ShowDialog() == true)
                {
                    ProgramExercise newProgramExercise = paramsWindow.CurrentProgramExercise;
                    
                    // Убедимся, что ExerciseID установлен правильно, а навигационное свойство Exercise
                    // не будет вызывать проблем при сохранении в новом контексте.
                    // Если ProgramExerciseParamsWindow уже установил newProgramExercise.ExerciseID,
                    // и если newProgramExercise.Exercise указывает на chosenExercise (из другого контекста),
                    // то перед добавлением в список, который будет сохранен, лучше убрать эту прямую ссылку на объект.
                    // EF Core свяжет по ExerciseID.
                    
                    // Это изменение, которое должно помочь:
                    if (newProgramExercise.Exercise != null && newProgramExercise.Exercise.ExerciseID != newProgramExercise.ExerciseID)
                    {
                        // На всякий случай, если ExerciseID в newProgramExercise не совпадает с Exercise.ExerciseID
                        newProgramExercise.ExerciseID = newProgramExercise.Exercise.ExerciseID;
                    }
                    newProgramExercise.Exercise = null; // <--- КЛЮЧЕВОЕ ИЗМЕНЕНИЕ! Отсоединяем объект Exercise.

                    ProgramExercisesList.Add(newProgramExercise);
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

            if (selectedProgramExercise.Exercise == null && selectedProgramExercise.ExerciseID > 0)
            {
                 // Попытка загрузить Exercise, если он не был загружен.
                 // Это не идеальное место, лучше грузить все сразу.
                 using(var tempCtx = new FitnessDbContext())
                 {
                    selectedProgramExercise.Exercise = tempCtx.Exercises.Find(selectedProgramExercise.ExerciseID);
                 }
                 if (selectedProgramExercise.Exercise == null)
                 {
                    MessageBox.Show("Ошибка: детали основного упражнения не могут быть загружены.", "Ошибка данных");
                    return;
                 }
            }
            
            ProgramExerciseParamsWindow paramsWindow = new ProgramExerciseParamsWindow(selectedProgramExercise);
            paramsWindow.Owner = this;

            if (paramsWindow.ShowDialog() == true)
            {
                // selectedProgramExercise уже изменен
                // Для обновления отображения в ListView, если ProgramExercise не INotifyPropertyChanged:
                ResortProgramExercisesList(); 
                ProgramExercisesListView.Items.Refresh(); 
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
            }
        }

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