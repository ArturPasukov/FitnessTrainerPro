using System.Windows;
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore; // Понадобится для обработки исключений EF Core, если будем детализировать

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
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                            // Обновить другие поля позже
                            dbContext.SaveChanges();
                        }
                    }
                    LoadClients();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка редактирования клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    }
                    LoadClients();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}