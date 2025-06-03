using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
                    // Для DateTimePoint, значение value - это DateTime.Ticks
                    Labeler = value => new DateTime((long)value).ToString("dd.MM.yy"), 
                    // Единица измерения - это длительность одного "шага" на оси в Ticks
                    UnitWidth = TimeSpan.FromDays(1).Ticks, 
                    // Минимальный шаг между метками также в Ticks
                    MinStep = TimeSpan.FromDays(1).Ticks, 
                    TextSize = 10
                }
            };
            WeightChart.YAxes = new List<Axis>
            {
                new Axis
                {
                    Name = "Вес, кг",
                    MinLimit = 0, // Начнем с 0, график сам подберет верхний предел
                    // MaxLimit = 150, // Можно задать максимальный предел, если нужно
                    TextSize = 10
                }
            };
            WeightChart.Series = new ISeries[0]; // Изначально график пуст
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadClientsIntoComboBox();
            EndDatePicker.SelectedDate = DateTime.Today;
            StartDatePicker.SelectedDate = DateTime.Today.AddMonths(-1);
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
        
        private bool IsUIDataReadyForChart()
        {
            return ClientsComboBox.SelectedItem != null && 
                   StartDatePicker.SelectedDate.HasValue && 
                   EndDatePicker.SelectedDate.HasValue;
        }

        private void LoadWeightChartData()
        {
            if (!IsUIDataReadyForChart())
            {
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
                        // Преобразуем замеры в точки для графика
                        // DateTimePoint(DateTime date, double value)
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
                                // TooltipLabelFormatter = (point) => $"{new DateTime((long)point.SecondaryValue).ToString("dd.MM.yyyy")}: {point.PrimaryValue} кг" // Для кастомных тултипов
                            }
                        };
                    }
                    else
                    {
                        WeightChart.Series = new ISeries[0]; 
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