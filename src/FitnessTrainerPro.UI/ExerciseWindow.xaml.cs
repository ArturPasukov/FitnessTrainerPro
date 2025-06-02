using System.Windows;
using FitnessTrainerPro.Core.Models; // Для Exercise

namespace FitnessTrainerPro.UI
{
    public partial class ExerciseWindow : Window
    {
        public Exercise CurrentExercise { get; private set; }

        // Конструктор для нового упражнения
        public ExerciseWindow()
        {
            InitializeComponent();
            CurrentExercise = new Exercise(); // Создаем новый пустой объект
            this.DataContext = CurrentExercise; // Для простой привязки, если решим использовать
        }

        // Конструктор для редактирования существующего упражнения
        public ExerciseWindow(Exercise exerciseToEdit)
        {
            InitializeComponent();
            CurrentExercise = exerciseToEdit; // Используем переданный объект
            
            // Заполняем поля данными из объекта
            NameTextBox.Text = CurrentExercise.Name;
            MuscleGroupTextBox.Text = CurrentExercise.MuscleGroup;
            DescriptionTextBox.Text = CurrentExercise.Description;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Обновляем свойства объекта CurrentExercise из полей TextBox
            CurrentExercise.Name = NameTextBox.Text;
            CurrentExercise.MuscleGroup = MuscleGroupTextBox.Text;
            CurrentExercise.Description = DescriptionTextBox.Text;
            
            // Простая валидация
            if (string.IsNullOrWhiteSpace(CurrentExercise.Name))
            {
                MessageBox.Show("Название упражнения не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.DialogResult = true; // Указываем, что пользователь нажал OK
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Указываем, что пользователь нажал Отмена
        }
    }
}