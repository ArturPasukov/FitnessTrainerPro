using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls; // Для SelectionChangedEventArgs
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

            // Настройка осей для графика веса
            WeightChart.XAxes = new List<Axis>
            {
                new Axis
                {
                    Name = "Дата замера",
                    Labeler = value => new DateTime((long)value).ToString("dd.MM.yy"), 
                    UnitWidth = TimeSpan.FromDays(1).Ticks, 
                    MinStep = TimeSpan.FromDays(1).Ticks,
                    TextSize = 10 // Можно настроить размер шрифта меток
                }
            };
            WeightChart.YAxes = new List<Axis>
            {
                new Axis
                {
                    Name = "Вес, кг",
                    MinLimit = 0, // Начнем с 0 или можно настроить динамически
                    TextSize = 10 // Можно настроить размер шрифта меток
                }
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadClientsIntoComboBox();
            EndDatePicker.SelectedDate = DateTime.Today;
            StartDatePicker.SelectedDate = DateTime.Today.AddMonths(-1);
            // LoadWeightChartData(); // Вызовется при изменении SelectedIndex в ComboBox
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
                        // Если клиентов нет, очищаем график
                        WeightChart.Series = new ISeries[0];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка клиентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClientsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Загружаем данные, только если все необходимые фильтры установлены
            if (ClientsComboBox.SelectedItem != null && StartDatePicker.SelectedDate.HasValue && EndDatePicker.SelectedDate.HasValue)
            {
                LoadWeightChartData();
            }
        }

        private void Dates_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientsComboBox.SelectedItem != null && StartDatePicker.SelectedDate.HasValue && EndDatePicker.SelectedDate.HasValue)
            {
                LoadWeightChartData();
            }
        }

        private void LoadWeightChartData()
        {
            if (ClientsComboBox.SelectedItem is not Client selectedClient ||
                !StartDatePicker.SelectedDate.HasValue ||
                !EndDatePicker.SelectedDate.HasValue)
            {
                WeightChart.Series = new ISeries[0]; // Очищаем график, если нет данных для загрузки
                return;
            }

            DateTime startDate = StartDatePicker.SelectedDate.Value.Date; // Берем только дату, без времени
            DateTime endDate = EndDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1); // Конец дня (23:59:59.9999999)

            if (startDate > endDate)
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
                                    m.WeightKg.HasValue && // Только замеры с указанным весом
                                    m.MeasurementDate >= startDate &&
                                    m.MeasurementDate <= endDate)
                        .OrderBy(m => m.MeasurementDate)
                        .ToList();

                    if (measurements.Any())
                    {
                        // Преобразуем замеры в точки для графика (DateTimePoint для временной оси)
                        var dataPoints = measurements.Select(m => new DateTimePoint(m.MeasurementDate, (double)m.WeightKg!.Value)).ToList();
                        
                        WeightChart.Series = new ISeries[]
                        {
                            new LineSeries<DateTimePoint>
                            {
                                Name = "Вес клиента",
                                Values = dataPoints,
                                GeometrySize = 10, // Размер точек на графике
                                Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2 },
                                Fill = null, // Без заливки под линией
                                GeometryStroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2 }
                            }
                        };
                    }
                    else
                    {
                        WeightChart.Series = new ISeries[0]; // Нет данных для отображения
                        // MessageBox.Show("Нет данных о весе для выбранного клиента в указанном диапазоне дат.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных для графика: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                WeightChart.Series = new ISeries[0]; // Очищаем график в случае ошибки
            }
        }
    }
}