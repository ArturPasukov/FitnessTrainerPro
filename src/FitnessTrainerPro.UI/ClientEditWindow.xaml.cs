using System;
using System.Collections.Generic; // Для List<T>
using System.Linq;
using System.Windows;
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Data;
using Microsoft.EntityFrameworkCore; // Для .Include()

namespace FitnessTrainerPro.UI
{
    public partial class ClientEditWindow : Window
    {
        public Client CurrentClient { get; private set; }
        // Используем List<T>, так как пока не планируем динамическое добавление/удаление программ из этого списка.
        // Если бы планировали, лучше был бы ObservableCollection<ClientAssignedProgram>.
        public List<ClientAssignedProgram> AssignedProgramsList { get; set; }

        // Конструктор для нового клиента
        public ClientEditWindow()
        {
            InitializeComponent();
            CurrentClient = new Client();
            AssignedProgramsList = new List<ClientAssignedProgram>(); // Инициализируем пустой список
            // Привязка к ListView будет сделана в XAML через ItemsSource="{Binding AssignedProgramsList}"
            // или мы можем установить ее здесь, если DataContext установлен на это окно.
            // Проще всего будет установить DataContext = this; в конструкторе и использовать Binding в XAML.
            // Но для ListView, который мы добавили вручную, можно и напрямую:
            AssignedProgramsListView.ItemsSource = AssignedProgramsList; 
            // DateOfBirthPicker.SelectedDate = DateTime.Today; 
        }

        // Конструктор для редактирования существующего клиента
        public ClientEditWindow(Client clientToEdit)
        {
            InitializeComponent();
            CurrentClient = clientToEdit;
            
            // Заполняем основные поля клиента
            FirstNameTextBox.Text = CurrentClient.FirstName;
            LastNameTextBox.Text = CurrentClient.LastName;
            DateOfBirthPicker.SelectedDate = CurrentClient.DateOfBirth;
            PhoneNumberTextBox.Text = CurrentClient.PhoneNumber;
            EmailTextBox.Text = CurrentClient.Email;            
            GoalsTextBox.Text = CurrentClient.Goals;   

            // Инициализируем список для назначенных программ
            AssignedProgramsList = new List<ClientAssignedProgram>(); 
            AssignedProgramsListView.ItemsSource = AssignedProgramsList; // Привязываем к ListView
            // Загрузка самих программ произойдет в Window_Loaded
        }

        // Обработчик события Loaded для окна, который мы добавили в XAML
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Загружаем назначенные программы только если это редактирование существующего клиента
            // и у него есть ClientID (т.е. он уже сохранен в базе)
            if (CurrentClient != null && CurrentClient.ClientID > 0)
            {
                LoadAssignedProgramsForClient();
            }
        }

        private void LoadAssignedProgramsForClient()
        {
            try
            {
                using (var dbContext = new FitnessDbContext())
                {
                    // Загружаем назначенные программы для текущего клиента,
                    // включая связанную информацию о самой программе тренировок (WorkoutProgram)
                    // чтобы отобразить ее имя в ListView.
                    var assignments = dbContext.ClientAssignedPrograms
                                          .Include(cap => cap.WorkoutProgram) // ВАЖНО: Загружаем связанную WorkoutProgram
                                          .Where(cap => cap.ClientID == CurrentClient.ClientID)
                                          .OrderByDescending(cap => cap.StartDate) // Сначала более новые назначения
                                          .ToList();
                    
                    // Обновляем список в UI
                    AssignedProgramsList.Clear(); // Очищаем, если там что-то было (хотя при первой загрузке он пуст)
                    foreach (var assignment in assignments)
                    {
                        AssignedProgramsList.Add(assignment);
                    }
                    // Если бы AssignedProgramsList был ObservableCollection, ListView обновился бы сам.
                    // Для List<T> нужно "передернуть" ItemsSource, чтобы ListView обновился.
                    AssignedProgramsListView.ItemsSource = null; // Сначала сбрасываем
                    AssignedProgramsListView.ItemsSource = AssignedProgramsList; // Затем присваиваем обновленный список
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки назначенных программ: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Сохраняем основные данные клиента
            CurrentClient.FirstName = FirstNameTextBox.Text;
            CurrentClient.LastName = LastNameTextBox.Text;
            CurrentClient.DateOfBirth = DateOfBirthPicker.SelectedDate;
            CurrentClient.PhoneNumber = PhoneNumberTextBox.Text;
            CurrentClient.Email = EmailTextBox.Text;            
            CurrentClient.Goals = GoalsTextBox.Text;             
            
            if (string.IsNullOrWhiteSpace(CurrentClient.FirstName) || string.IsNullOrWhiteSpace(CurrentClient.LastName))
            {
                MessageBox.Show("Имя и фамилия клиента не могут быть пустыми.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // Валидацию для Email, PhoneNumber можно добавить здесь

            // Список AssignedPrograms мы здесь не изменяем, он только для отображения.
            // Управление назначениями (добавление/удаление) происходит в другом месте (ClientManagementWindow -> AssignProgramWindow).
            // Если бы мы хотели редактировать детали назначения прямо здесь, потребовалась бы другая логика.

            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}