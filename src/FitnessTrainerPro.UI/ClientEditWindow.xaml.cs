using System;
using System.Collections.Generic; // Для List
using System.Collections.ObjectModel; // Для ObservableCollection
using System.Linq;
using System.Windows;
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessTrainerPro.UI
{
    public partial class ClientEditWindow : Window
    {
        public Client CurrentClient { get; private set; }
        
        // Список для назначенных программ
        public ObservableCollection<ClientAssignedProgram> AssignedProgramsList { get; set; }

        // НОВЫЙ СПИСОК для замеров клиента
        public ObservableCollection<ClientMeasurement> MeasurementsList { get; set; }

        // Конструктор для НОВОГО клиента
        public ClientEditWindow()
        {
            InitializeComponent();
            CurrentClient = new Client();
            
            AssignedProgramsList = new ObservableCollection<ClientAssignedProgram>();
            AssignedProgramsListView.ItemsSource = AssignedProgramsList; // Привязка для вкладки "Информация и Программы"

            MeasurementsList = new ObservableCollection<ClientMeasurement>(); // Инициализируем список замеров
            MeasurementsListView.ItemsSource = MeasurementsList;       // Привязка для вкладки "Замеры"
        }

        // Конструктор для РЕДАКТИРОВАНИЯ клиента
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

            // Инициализируем списки для вкладок
            AssignedProgramsList = new ObservableCollection<ClientAssignedProgram>(); 
            AssignedProgramsListView.ItemsSource = AssignedProgramsList;
            
            MeasurementsList = new ObservableCollection<ClientMeasurement>(); 
            MeasurementsListView.ItemsSource = MeasurementsList;
            // Загрузка данных для списков произойдет в Window_Loaded
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Загружаем связанные данные только если это редактирование существующего клиента
            if (CurrentClient != null && CurrentClient.ClientID > 0) 
            {
                LoadAssignedProgramsForClient();
                LoadMeasurementsForClient(); // НОВЫЙ ВЫЗОВ
            }
        }

        private void LoadAssignedProgramsForClient()
        {
            try
            {
                using (var dbContext = new FitnessDbContext())
                {
                    var assignments = dbContext.ClientAssignedPrograms
                                          .Include(cap => cap.WorkoutProgram)
                                          .Where(cap => cap.ClientID == CurrentClient.ClientID)
                                          .OrderByDescending(cap => cap.StartDate)
                                          .ToList();
                    
                    AssignedProgramsList.Clear(); 
                    foreach (var assignment in assignments)
                    {
                        AssignedProgramsList.Add(assignment);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки назначенных программ: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // НОВЫЙ МЕТОД для загрузки замеров
        private void LoadMeasurementsForClient()
        {
            try
            {
                using (var dbContext = new FitnessDbContext())
                {
                    var measurements = dbContext.ClientMeasurements
                                           .Where(m => m.ClientID == CurrentClient.ClientID)
                                           .OrderByDescending(m => m.MeasurementDate) 
                                           .ToList();
                    
                    MeasurementsList.Clear();
                    foreach (var measurement in measurements)
                    {
                        MeasurementsList.Add(measurement);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки замеров клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
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
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        // --- НОВЫЕ ОБРАБОТЧИКИ ДЛЯ КНОПОК УПРАВЛЕНИЯ ЗАМЕРАМИ ---
        private void AddMeasurementButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentClient == null || CurrentClient.ClientID == 0)
            {
                // Это может произойти, если пытаемся добавить замер для еще не сохраненного нового клиента
                MessageBox.Show("Сначала сохраните нового клиента, прежде чем добавлять замеры.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                // Можно переключить фокус на первую вкладку, если есть TabControl
                // MainTabControl.SelectedIndex = 0; 
                return;
            }

            MeasurementEditWindow measurementWindow = new MeasurementEditWindow(CurrentClient.ClientID);
            measurementWindow.Owner = this;

            if (measurementWindow.ShowDialog() == true)
            {
                ClientMeasurement newMeasurement = measurementWindow.CurrentMeasurement;
                try
                {
                    using (var dbContext = new FitnessDbContext())
                    {
                        dbContext.ClientMeasurements.Add(newMeasurement);
                        dbContext.SaveChanges();
                    }
                    LoadMeasurementsForClient(); // Обновляем список замеров в UI
                }
                catch (DbUpdateException dbEx)
                {
                    string errorMessage = $"Ошибка добавления замера (БД): {dbEx.Message}";
                    if (dbEx.InnerException != null) errorMessage += $"\n\nВнутренняя ошибка БД: {dbEx.InnerException.Message}";
                    MessageBox.Show(errorMessage, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    string errorMessage = $"Общая ошибка добавления замера: {ex.Message}";
                    if (ex.InnerException != null) errorMessage += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                    MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditMeasurementButton_Click(object sender, RoutedEventArgs e)
        {
            ClientMeasurement? selectedMeasurement = MeasurementsListView.SelectedItem as ClientMeasurement;
            if (selectedMeasurement == null)
            {
                MessageBox.Show("Пожалуйста, выберите замер для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MeasurementEditWindow measurementWindow = new MeasurementEditWindow(selectedMeasurement);
            measurementWindow.Owner = this;

            if (measurementWindow.ShowDialog() == true)
            {
                try
                {
                    using (var dbContext = new FitnessDbContext())
                    {
                        var measurementInDb = dbContext.ClientMeasurements.Find(selectedMeasurement.MeasurementID);
                        if (measurementInDb != null)
                        {
                            measurementInDb.MeasurementDate = measurementWindow.CurrentMeasurement.MeasurementDate;
                            measurementInDb.WeightKg = measurementWindow.CurrentMeasurement.WeightKg;
                            measurementInDb.ChestCm = measurementWindow.CurrentMeasurement.ChestCm;
                            measurementInDb.WaistCm = measurementWindow.CurrentMeasurement.WaistCm;
                            measurementInDb.HipsCm = measurementWindow.CurrentMeasurement.HipsCm;
                            measurementInDb.Notes = measurementWindow.CurrentMeasurement.Notes;
                            
                            dbContext.SaveChanges();
                        }
                    }
                    LoadMeasurementsForClient(); 
                }
                catch (DbUpdateException dbEx)
                {
                    string errorMessage = $"Ошибка редактирования замера (БД): {dbEx.Message}";
                    if (dbEx.InnerException != null) errorMessage += $"\n\nВнутренняя ошибка БД: {dbEx.InnerException.Message}";
                    MessageBox.Show(errorMessage, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    string errorMessage = $"Общая ошибка редактирования замера: {ex.Message}";
                    if (ex.InnerException != null) errorMessage += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                    MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteMeasurementButton_Click(object sender, RoutedEventArgs e)
        {
            ClientMeasurement? selectedMeasurement = MeasurementsListView.SelectedItem as ClientMeasurement;
            if (selectedMeasurement == null)
            {
                MessageBox.Show("Пожалуйста, выберите замер для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Удалить замер от {selectedMeasurement.MeasurementDate:dd.MM.yyyy}?",
                                                     "Подтверждение удаления",
                                                     MessageBoxButton.YesNo,
                                                     MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var dbContext = new FitnessDbContext())
                    {
                        var measurementToDelete = dbContext.ClientMeasurements.Find(selectedMeasurement.MeasurementID);
                        if (measurementToDelete != null)
                        {
                            dbContext.ClientMeasurements.Remove(measurementToDelete);
                            dbContext.SaveChanges();
                        }
                    }
                    LoadMeasurementsForClient(); 
                }
                catch (DbUpdateException dbEx)
                {
                    string errorMessage = $"Ошибка удаления замера (БД): {dbEx.Message}";
                    if (dbEx.InnerException != null) errorMessage += $"\n\nВнутренняя ошибка БД: {dbEx.InnerException.Message}";
                    MessageBox.Show(errorMessage, "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    string errorMessage = $"Общая ошибка удаления замера: {ex.Message}";
                    if (ex.InnerException != null) errorMessage += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                    MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}