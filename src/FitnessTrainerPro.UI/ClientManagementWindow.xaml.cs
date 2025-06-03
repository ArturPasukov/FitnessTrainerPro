using System.Windows;
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore; // Для DbUpdateException
using System; // Для Exception

namespace FitnessTrainerPro.UI
{
    public partial class ClientManagementWindow : Window
    {
        public ClientManagementWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadClients();
        }

        private void LoadClients()
        {
            try
            {
                using (var dbContext = new FitnessDbContext())
                {
                    ClientsListView.ItemsSource = dbContext.Clients.ToList();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка клиентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddClientButton_Click(object sender, RoutedEventArgs e)
        {
            ClientEditWindow clientEditWindow = new ClientEditWindow();
            clientEditWindow.Owner = this;

            if (clientEditWindow.ShowDialog() == true)
            {
                Client newClient = clientEditWindow.CurrentClient;
                try
                {
                    using (var dbContext = new FitnessDbContext())
                    {
                        dbContext.Clients.Add(newClient);
                        dbContext.SaveChanges();
                    }
                    LoadClients();
                }
                catch (System.Exception ex) // Добавим более детальный вывод ошибки
                {
                    string errorMessage = $"Ошибка добавления клиента: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                    }
                    MessageBox.Show(errorMessage, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditClientButton_Click(object sender, RoutedEventArgs e)
        {
            Client? selectedClient = ClientsListView.SelectedItem as Client;
            if (selectedClient == null)
            {
                MessageBox.Show("Пожалуйста, выберите клиента для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ClientEditWindow clientEditWindow = new ClientEditWindow(selectedClient);
            clientEditWindow.Owner = this;

            if (clientEditWindow.ShowDialog() == true)
            {
                try
                {
                    using (var dbContext = new FitnessDbContext())
                    {
                        var clientInDb = dbContext.Clients.Find(selectedClient.ClientID);
                        if (clientInDb != null)
                        {
                            clientInDb.FirstName = clientEditWindow.CurrentClient.FirstName;
                            clientInDb.LastName = clientEditWindow.CurrentClient.LastName;
                            clientInDb.DateOfBirth = clientEditWindow.CurrentClient.DateOfBirth;
                            clientInDb.PhoneNumber = clientEditWindow.CurrentClient.PhoneNumber;
                            clientInDb.Email = clientEditWindow.CurrentClient.Email;            
                            clientInDb.Goals = clientEditWindow.CurrentClient.Goals;             
                            
                            dbContext.SaveChanges();
                        }
                        else
                        {
                            MessageBox.Show("Клиент не найден в базе данных. Возможно, он был удален.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    LoadClients();
                }
                catch (DbUpdateException dbEx) // Более специфичная обработка для ошибок БД
                {
                    string errorMessage = $"Ошибка обновления клиента в базе данных: {dbEx.Message}";
                    if (dbEx.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутренняя ошибка БД: {dbEx.InnerException.Message}";
                    }
                    MessageBox.Show(errorMessage, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.Exception ex) // Общая обработка
                {
                    string errorMessage = $"Общая ошибка при редактировании клиента: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                    }
                    MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteClientButton_Click(object sender, RoutedEventArgs e)
        {
            Client? selectedClient = ClientsListView.SelectedItem as Client;
            if (selectedClient == null)
            {
                MessageBox.Show("Пожалуйста, выберите клиента для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Вы уверены, что хотите удалить клиента '{selectedClient.FirstName} {selectedClient.LastName}'?",
                                                     "Подтверждение удаления",
                                                     MessageBoxButton.YesNo,
                                                     MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var dbContext = new FitnessDbContext())
                    {
                        var clientToDelete = dbContext.Clients.Find(selectedClient.ClientID);
                        if (clientToDelete != null)
                        {
                            dbContext.Clients.Remove(clientToDelete);
                            dbContext.SaveChanges();
                        }
                         else
                        {
                            MessageBox.Show("Клиент не найден в базе данных. Возможно, он уже был удален.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    LoadClients();
                }
                catch (DbUpdateException dbEx)
                {
                    string errorMessage = $"Ошибка удаления клиента из базы данных: {dbEx.Message}";
                     if (dbEx.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутренняя ошибка БД: {dbEx.InnerException.Message}";
                    }
                    MessageBox.Show(errorMessage, "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.Exception ex)
                {
                     string errorMessage = $"Общая ошибка при удалении клиента: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                    }
                    MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // НОВЫЙ МЕТОД - ОБРАБОТЧИК КНОПКИ "НАЗНАЧИТЬ ПРОГРАММУ"
        private void AssignProgramButton_Click(object sender, RoutedEventArgs e)
        {
            Client? selectedClient = ClientsListView.SelectedItem as Client;
            if (selectedClient == null)
            {
                MessageBox.Show("Пожалуйста, выберите клиента, которому хотите назначить программу.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            AssignProgramWindow assignWindow = new AssignProgramWindow(selectedClient);
            assignWindow.Owner = this;

            if (assignWindow.ShowDialog() == true)
            {
                ClientAssignedProgram newAssignment = assignWindow.AssignmentDetails;
                
                try
                {
                    using (var dbContext = new FitnessDbContext())
                    {
                        // TODO: Добавить проверку на уже существующие активные программы для этого клиента, если нужно.
                        // Пока просто добавляем.
                        
                        dbContext.ClientAssignedPrograms.Add(newAssignment);
                        dbContext.SaveChanges();
                        
                        // Получаем имя программы для сообщения (опционально, но информативно)
                        string programName = dbContext.WorkoutPrograms.Find(newAssignment.ProgramID)?.Name ?? "Неизвестная программа";
                        MessageBox.Show($"Программа '{programName}' успешно назначена клиенту '{selectedClient.FirstName} {selectedClient.LastName}'.", 
                                        "Назначение успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    // LoadClients(); // Пока нечего обновлять в списке клиентов от этого действия.
                                    // Позже, когда будем отображать назначенные программы, это может понадобиться.
                }
                catch (DbUpdateException dbEx)
                {
                    string errorMessage = $"Ошибка назначения программы (БД): {dbEx.Message}";
                    if (dbEx.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутренняя ошибка БД: {dbEx.InnerException.Message}";
                    }
                    MessageBox.Show(errorMessage, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    string errorMessage = $"Общая ошибка назначения программы: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                    }
                    MessageBox.Show(errorMessage, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}