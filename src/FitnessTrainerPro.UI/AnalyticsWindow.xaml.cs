using System;
using System.Linq;
using System.Windows;
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Data;
using System.Globalization; // Для форматирования чисел
using System.Windows.Controls; // <--- ДОБАВЛЕНА ЭТА ДИРЕКТИВА USING

namespace FitnessTrainerPro.UI
{
    public partial class AnalyticsWindow : Window
    {
        public AnalyticsWindow()
        {
            InitializeComponent();
            // Инициализация графика больше не нужна
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadClientsIntoComboBox();
            EndDatePicker.SelectedDate = DateTime.Today;
            StartDatePicker.SelectedDate = DateTime.Today.AddMonths(-1);
            ClearResults(); // Очищаем текстовые поля при загрузке
        }

        private void LoadClientsIntoComboBox()
        {
            try
            {
                using (var dbContext = new FitnessDbContext())
                {
                    var clients = dbContext.Clients.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToList();
                    ClientsComboBox.ItemsSource = clients;
                    if (clients.Any())
                    {
                        ClientsComboBox.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка клиентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void ClearResults()
        {
            WeightChangeTextBlock.Text = "-";
            ChestChangeTextBlock.Text = "-";
            WaistChangeTextBlock.Text = "-";
            HipsChangeTextBlock.Text = "-";
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            CalculateAndDisplayPercentageChange();
        }

        private void CalculateAndDisplayPercentageChange()
        {
            ClearResults(); 

            if (ClientsComboBox.SelectedItem is not Client selectedClient ||
                !StartDatePicker.SelectedDate.HasValue ||
                !EndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите клиента и обе даты.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            DateTime startDate = StartDatePicker.SelectedDate.Value.Date;
            DateTime endDate = EndDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1); 

            if (startDate > endDate.Date)
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания.", "Ошибка диапазона дат", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var dbContext = new FitnessDbContext())
                {
                    var allMeasurementsInRange = dbContext.ClientMeasurements
                        .Where(m => m.ClientID == selectedClient.ClientID &&
                                    m.MeasurementDate >= startDate &&
                                    m.MeasurementDate <= endDate)
                        .OrderBy(m => m.MeasurementDate)
                        .ToList();

                    if (!allMeasurementsInRange.Any())
                    {
                        MessageBox.Show("Нет замеров для выбранного клиента в указанном диапазоне дат.", "Нет данных", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    var firstWeightMeasurement = allMeasurementsInRange.FirstOrDefault(m => m.WeightKg.HasValue);
                    var lastWeightMeasurement = allMeasurementsInRange.LastOrDefault(m => m.WeightKg.HasValue);
                    CalculateAndDisplayChange(firstWeightMeasurement?.WeightKg, lastWeightMeasurement?.WeightKg, WeightChangeTextBlock, "кг");

                    var firstChestMeasurement = allMeasurementsInRange.FirstOrDefault(m => m.ChestCm.HasValue);
                    var lastChestMeasurement = allMeasurementsInRange.LastOrDefault(m => m.ChestCm.HasValue);
                    CalculateAndDisplayChange(firstChestMeasurement?.ChestCm, lastChestMeasurement?.ChestCm, ChestChangeTextBlock, "см");

                    var firstWaistMeasurement = allMeasurementsInRange.FirstOrDefault(m => m.WaistCm.HasValue);
                    var lastWaistMeasurement = allMeasurementsInRange.LastOrDefault(m => m.WaistCm.HasValue);
                    CalculateAndDisplayChange(firstWaistMeasurement?.WaistCm, lastWaistMeasurement?.WaistCm, WaistChangeTextBlock, "см");

                    var firstHipsMeasurement = allMeasurementsInRange.FirstOrDefault(m => m.HipsCm.HasValue);
                    var lastHipsMeasurement = allMeasurementsInRange.LastOrDefault(m => m.HipsCm.HasValue);
                    CalculateAndDisplayChange(firstHipsMeasurement?.HipsCm, lastHipsMeasurement?.HipsCm, HipsChangeTextBlock, "см");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка расчета прогресса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateAndDisplayChange(double? startValue, double? endValue, TextBlock targetTextBlock, string unit)
        {
            if (startValue.HasValue && endValue.HasValue && startValue.Value != 0)
            {
                double change = endValue.Value - startValue.Value;
                double percentageChange = (change / startValue.Value) * 100;
                string sign = change >= 0 ? "+" : ""; // Знак '+' только для положительных или нуля
                targetTextBlock.Text = $"{startValue.Value:F1} {unit} -> {endValue.Value:F1} {unit} ({sign}{change:F1} {unit}, {sign}{percentageChange:F1}%)";
            }
            else if (startValue.HasValue && endValue.HasValue && startValue.Value == 0 && endValue.Value != 0)
            {
                 targetTextBlock.Text = $"Начало: 0 {unit}, Конец: {endValue.Value:F1} {unit} (изм. не рассч.)";
            }
            else if (startValue.HasValue && !endValue.HasValue)
            {
                targetTextBlock.Text = $"Начало: {startValue.Value:F1} {unit}, конечного замера нет";
            }
             else if (!startValue.HasValue && endValue.HasValue)
            {
                targetTextBlock.Text = $"Конец: {endValue.Value:F1} {unit}, начального замера нет";
            }
            else
            {
                targetTextBlock.Text = "Недостаточно данных";
            }
        }
    }
}