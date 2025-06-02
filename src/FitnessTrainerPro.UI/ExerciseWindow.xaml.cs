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
            // Если в будущем будешь использовать DataBinding напрямую к свойствам CurrentExercise в XAML,
            // то можно раскомментировать:
            // this.DataContext = CurrentExercise; 
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
            VideoUrlTextBox.Text = CurrentExercise.VideoUrl; // Добавлено
            EquipmentNeededTextBox.Text = CurrentExercise.EquipmentNeeded; // Добавлено
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Обновляем свойства объекта CurrentExercise из полей TextBox
            CurrentExercise.Name = NameTextBox.Text;
            CurrentExercise.MuscleGroup = MuscleGroupTextBox.Text;
            CurrentExercise.Description = DescriptionTextBox.Text;
            CurrentExercise.VideoUrl = VideoUrlTextBox.Text; // Добавлено
            CurrentExercise.EquipmentNeeded = EquipmentNeededTextBox.Text; // Добавлено
            
            // Простая валидация
            if (string.IsNullOrWhiteSpace(CurrentExercise.Name))
            {
                MessageBox.Show("Название упражнения не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Можно добавить валидацию для URL, если нужно
            // if (!string.IsNullOrWhiteSpace(CurrentExercise.VideoUrl) && 
            //     !System.Uri.IsWellFormedUriString(CurrentExercise.VideoUrl, System.UriKind.Absolute))
            // {
            //     MessageBox.Show("URL видео введен некорректно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            //     return;
            // }

            this.DialogResult = true; // Указываем, что пользователь нажал OK
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Указываем, что пользователь нажал Отмена
        }
    }
}