using System.Windows;
using FitnessTrainerPro.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;
using FitnessTrainerPro.Data; // <--- ДОБАВЛЕНО ЭТО

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
                    
                    if (newProgramExercise.Exercise != null && newProgramExercise.Exercise.ExerciseID != newProgramExercise.ExerciseID)
                    {
                        newProgramExercise.ExerciseID = newProgramExercise.Exercise.ExerciseID;
                    }
                    newProgramExercise.Exercise = null; // Теперь это присвоение корректно, т.к. свойство nullable

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

            // Если Exercise не был загружен (хотя должен быть через Include в WorkoutProgramManagementWindow)
            if (selectedProgramExercise.Exercise == null && selectedProgramExercise.ExerciseID > 0)
            {
                 try
                 {
                     using(var tempCtx = new FitnessDbContext()) // FitnessDbContext теперь должен быть виден
                     {
                        selectedProgramExercise.Exercise = tempCtx.Exercises.Find(selectedProgramExercise.ExerciseID);
                     }
                 }
                 catch (System.Exception ex)
                 {
                    MessageBox.Show($"Не удалось подгрузить детали упражнения: {ex.Message}", "Ошибка данных");
                    // Можно решить не продолжать, если Exercise критичен для редактирования параметров
                 }
                 
                 if (selectedProgramExercise.Exercise == null) // Повторная проверка после попытки загрузки
                 {
                    MessageBox.Show("Ошибка: детали основного упражнения не могут быть загружены для редактирования параметров.", "Ошибка данных");
                    return;
                 }
            }
            
            ProgramExerciseParamsWindow paramsWindow = new ProgramExerciseParamsWindow(selectedProgramExercise);
            paramsWindow.Owner = this;

            if (paramsWindow.ShowDialog() == true)
            {
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