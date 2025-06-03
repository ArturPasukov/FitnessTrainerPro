using System;
using System.Linq;
using System.Windows;
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Data;
// using System.Collections.Generic; // Может не понадобиться напрямую, если ToList() используется

namespace FitnessTrainerPro.UI
{
    public partial class AssignProgramWindow : Window
    {
        private Client _selectedClient; // Храним клиента, которому назначаем программу
        public ClientAssignedProgram AssignmentDetails { get; private set; } // Результат работы окна

        // Конструктор принимает клиента
        public AssignProgramWindow(Client client)
        {
            InitializeComponent();
            _selectedClient = client;

            // Инициализируем объект, который будем заполнять и возвращать
            AssignmentDetails = new ClientAssignedProgram
            {
                ClientID = _selectedClient.ClientID,
                // Client = _selectedClient, // Можно установить, но EF Core свяжет по ClientID
                StartDate = DateTime.Today, // Значение по умолчанию для даты начала
                IsActive = true             // По умолчанию программа активна
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Отображаем имя клиента
            if (_selectedClient != null)
            {
                ClientNameTextBlock.Text = $"{_selectedClient.FirstName} {_selectedClient.LastName}";
            }

            // Загружаем доступные программы тренировок в ComboBox
            LoadAvailablePrograms();

            // Устанавливаем значение по умолчанию для DatePicker'а даты начала
            StartDatePicker.SelectedDate = AssignmentDetails.StartDate; 
        }

        private void LoadAvailablePrograms()
        {
            try
            {
                using (var dbContext = new FitnessDbContext())
                {
                    ProgramsComboBox.ItemsSource = dbContext.WorkoutPrograms.OrderBy(p => p.Name).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка программ тренировок: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AssignButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (ProgramsComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите программу тренировок.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (StartDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите дату начала.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            WorkoutProgram? selectedProgram = ProgramsComboBox.SelectedItem as WorkoutProgram;
            if (selectedProgram == null) 
            {
                 MessageBox.Show("Выбран некорректный элемент программы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Заполняем объект AssignmentDetails данными из формы
            AssignmentDetails.ProgramID = selectedProgram.ProgramID;
            AssignmentDetails.StartDate = StartDatePicker.SelectedDate.Value;
            AssignmentDetails.EndDate = EndDatePicker.SelectedDate; // EndDatePicker.SelectedDate уже nullable DateTime?
            AssignmentDetails.TrainerNotesForClient = TrainerNotesTextBox.Text;
            AssignmentDetails.IsActive = true; 

            // Дополнительная валидация (например, дата окончания не раньше даты начала)
            if (AssignmentDetails.EndDate.HasValue && AssignmentDetails.EndDate.Value < AssignmentDetails.StartDate)
            {
                MessageBox.Show("Дата окончания не может быть раньше даты начала.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            this.DialogResult = true; // Устанавливаем результат диалога в true, окно закроется
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Устанавливаем результат диалога в false, окно закроется
        }
    }
}