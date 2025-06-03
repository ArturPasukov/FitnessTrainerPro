using System.Windows;
using FitnessTrainerPro.Core.Models; // Для Client
using System; // Для DateTime

namespace FitnessTrainerPro.UI
{
    public partial class ClientEditWindow : Window
    {
        public Client CurrentClient { get; private set; }

        // Конструктор для нового клиента
        public ClientEditWindow()
        {
            InitializeComponent();
            CurrentClient = new Client();
            // DateOfBirthPicker.SelectedDate = DateTime.Today; 
        }

        // Конструктор для редактирования существующего клиента
        public ClientEditWindow(Client clientToEdit)
        {
            InitializeComponent();
            CurrentClient = clientToEdit;
            
            FirstNameTextBox.Text = CurrentClient.FirstName;
            LastNameTextBox.Text = CurrentClient.LastName;
            DateOfBirthPicker.SelectedDate = CurrentClient.DateOfBirth;
            PhoneNumberTextBox.Text = CurrentClient.PhoneNumber; // ДОБАВЛЕНО
            EmailTextBox.Text = CurrentClient.Email;             // ДОБАВЛЕНО
            GoalsTextBox.Text = CurrentClient.Goals;             // ДОБАВЛЕНО
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentClient.FirstName = FirstNameTextBox.Text;
            CurrentClient.LastName = LastNameTextBox.Text;
            CurrentClient.DateOfBirth = DateOfBirthPicker.SelectedDate;
            CurrentClient.PhoneNumber = PhoneNumberTextBox.Text; // ДОБАВЛЕНО
            CurrentClient.Email = EmailTextBox.Text;             // ДОБАВЛЕНО
            CurrentClient.Goals = GoalsTextBox.Text;             // ДОБАВЛЕНО
            
            if (string.IsNullOrWhiteSpace(CurrentClient.FirstName) || string.IsNullOrWhiteSpace(CurrentClient.LastName))
            {
                MessageBox.Show("Имя и фамилия клиента не могут быть пустыми.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // Можно добавить валидацию для Email, PhoneNumber и т.д.

            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}