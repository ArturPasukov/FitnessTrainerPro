using System.Windows;
using FitnessTrainerPro.Core.Models;
// using System.Collections.Generic; // Для ProgramExercises
// using System.Linq;

namespace FitnessTrainerPro.UI
{
    public partial class WorkoutProgramEditWindow : Window
    {
        public WorkoutProgram CurrentProgram { get; private set; }
        // public List<ProgramExercise> ProgramExercisesList { get; set; } // Для хранения упражнений программы

        // Конструктор для новой программы
        public WorkoutProgramEditWindow()
        {
            InitializeComponent();
            CurrentProgram = new WorkoutProgram();
            // ProgramExercisesList = new List<ProgramExercise>();
            // TODO: Bind ProgramExercisesList to a ListView for exercises in program
        }

        // Конструктор для редактирования существующей программы
        public WorkoutProgramEditWindow(WorkoutProgram programToEdit)
        {
            InitializeComponent();
            CurrentProgram = programToEdit;
            // ProgramExercisesList = programToEdit.ProgramExercises.ToList(); // Копируем список

            ProgramNameTextBox.Text = CurrentProgram.Name;
            ProgramFocusTextBox.Text = CurrentProgram.Focus;
            ProgramDescriptionTextBox.Text = CurrentProgram.Description;
            
            // TODO: Populate ListView for exercises in program from ProgramExercisesList
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

            // TODO: Сохранить ProgramExercisesList обратно в CurrentProgram.ProgramExercises
            // CurrentProgram.ProgramExercises = new List<ProgramExercise>(ProgramExercisesList);

            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        // TODO: Методы для добавления, редактирования, удаления упражнений из ProgramExercisesList
    }
}