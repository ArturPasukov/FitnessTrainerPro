using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
// using System.Windows.Controls; // SelectionChangedEventArgs больше не нужен
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Data;
using LiveChartsCore; 
using LiveChartsCore.Defaults; 
using LiveChartsCore.SkiaSharpView; 
using LiveChartsCore.SkiaSharpView.Painting; 
using SkiaSharp; 

namespace FitnessTrainerPro.UI
{
    public partial class AnalyticsWindow : Window
    {
        public AnalyticsWindow()
        {
            InitializeComponent();
            InitializeWeightChart();
        }

        private void InitializeWeightChart()
        {
            WeightChart.XAxes = new List<Axis>
            {
                new Axis
                {
                    Name = "Дата замера",
                    Labeler = value => new DateTime((long)value).ToString("dd.MM.yy"), 
                    UnitWidth = TimeSpan.FromDays(1).Ticks, 
                    MinStep = TimeSpan.FromDays(1).Ticks,
                    TextSize = 10 
                }
            };
            WeightChart.YAxes = new List<Axis>
            {
                new Axis
                {
                    Name = "Вес, кг",
                    MinLimit = 0, 
                    TextSize = 10 
                }
            };
            WeightChart.Series = new ISeries[0];
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadClientsIntoComboBox();
            EndDatePicker.SelectedDate = DateTime.Today;
            StartDatePicker.SelectedDate = DateTime.Today.AddMonths(-1);
            // Не вызываем LoadWeightChartData здесь, ждем нажатия кнопки
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
                    else
                    {
                        WeightChart.Series = new ISeries[0];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка клиентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Методы ClientsComboBox_SelectionChanged и Dates_SelectedDateChanged УДАЛЕНЫ ИЛИ ЗАКОММЕНТИРОВАНЫ
        /* 
        private void ClientsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsUIDataReadyForChart())
            {
                LoadWeightChartData();
            }
        }

        private void Dates_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsUIDataReadyForChart())
            {
                LoadWeightChartData();
            }
        }
        */
        
        private bool IsUIDataReadyForChart() // Этот метод все еще полезен
        {
            return ClientsComboBox.SelectedItem != null && 
                   StartDatePicker.SelectedDate.HasValue && 
                   EndDatePicker.SelectedDate.HasValue;
        }

        // НОВЫЙ ОБРАБОТЧИК ДЛЯ КНОПКИ "ПОСТРОИТЬ ГРАФИК"
        private void PlotChartButton_Click(object sender, RoutedEventArgs e)
        {
            LoadWeightChartData(); // Вызываем загрузку и построение графика
        }

        private void LoadWeightChartData()
        {
            if (!IsUIDataReadyForChart())
            {
                // Можно показать сообщение, если пользователь нажал кнопку, но не выбрал все параметры
                MessageBox.Show("Пожалуйста, выберите клиента и обе даты для построения графика.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                WeightChart.Series = new ISeries[0]; 
                return;
            }

            Client selectedClient = (Client)ClientsComboBox.SelectedItem; 
            DateTime startDate = StartDatePicker.SelectedDate!.Value.Date; 
            DateTime endDate = EndDatePicker.SelectedDate!.Value.Date.AddDays(1).AddTicks(-1);

            if (startDate > endDate.Date) 
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания.", "Ошибка диапазона дат", MessageBoxButton.OK, MessageBoxImage.Warning);
                WeightChart.Series = new ISeries[0];
                return;
            }

            try
            {
                using (var dbContext = new FitnessDbContext())
                {
                    var measurements = dbContext.ClientMeasurements
                        .Where(m => m.ClientID == selectedClient.ClientID &&
                                    m.WeightKg.HasValue && 
                                    m.MeasurementDate >= startDate &&
                                    m.MeasurementDate <= endDate)
                        .OrderBy(m => m.MeasurementDate)
                        .ToList();

                    if (measurements.Any())
                    {
                        var dataPoints = measurements.Select(m => new DateTimePoint(m.MeasurementDate, m.WeightKg!.Value)).ToList();
                        
                        WeightChart.Series = new ISeries[]
                        {
                            new LineSeries<DateTimePoint>
                            {
                                Name = "Вес клиента",
                                Values = dataPoints,
                                GeometrySize = 8, 
                                Stroke = new SolidColorPaint(SKColors.DodgerBlue) { StrokeThickness = 3 },
                                Fill = null, 
                                GeometryStroke = new SolidColorPaint(SKColors.CornflowerBlue) { StrokeThickness = 3 }
                            }
                        };
                    }
                    else
                    {
                        WeightChart.Series = new ISeries[0]; 
                        MessageBox.Show("Нет данных о весе для выбранного клиента в указанном диапазоне дат.", "Нет данных", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных для графика: {ex.Message}\n{ex.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                WeightChart.Series = new ISeries[0]; 
            }
        }
    }
}