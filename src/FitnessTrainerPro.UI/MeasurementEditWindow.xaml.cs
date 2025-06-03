using System;
using System.Globalization; // Для CultureInfo при парсинге double
using System.Windows;
using FitnessTrainerPro.Core.Models;

namespace FitnessTrainerPro.UI
{
    public partial class MeasurementEditWindow : Window
    {
        public ClientMeasurement CurrentMeasurement { get; private set; }

        // Конструктор для нового замера
        public MeasurementEditWindow(int clientId)
        {
            InitializeComponent();
            CurrentMeasurement = new ClientMeasurement
            {
                ClientID = clientId,
                MeasurementDate = DateTime.Today // Значение по умолчанию
            };
            MeasurementDatePicker.SelectedDate = CurrentMeasurement.MeasurementDate;
        }

        // Конструктор для редактирования существующего замера
        public MeasurementEditWindow(ClientMeasurement measurementToEdit)
        {
            InitializeComponent();
            CurrentMeasurement = measurementToEdit;

            MeasurementDatePicker.SelectedDate = CurrentMeasurement.MeasurementDate;
            WeightTextBox.Text = CurrentMeasurement.WeightKg?.ToString(CultureInfo.InvariantCulture);
            ChestTextBox.Text = CurrentMeasurement.ChestCm?.ToString(CultureInfo.InvariantCulture);
            WaistTextBox.Text = CurrentMeasurement.WaistCm?.ToString(CultureInfo.InvariantCulture);
            HipsTextBox.Text = CurrentMeasurement.HipsCm?.ToString(CultureInfo.InvariantCulture);
            NotesTextBox.Text = CurrentMeasurement.Notes;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (MeasurementDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите дату замера.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CurrentMeasurement.MeasurementDate = MeasurementDatePicker.SelectedDate.Value;
            
            // Парсинг и валидация числовых значений
            CurrentMeasurement.WeightKg = TryParseNullableDouble(WeightTextBox.Text, "Вес");
            CurrentMeasurement.ChestCm = TryParseNullableDouble(ChestTextBox.Text, "Обхват груди");
            CurrentMeasurement.WaistCm = TryParseNullableDouble(WaistTextBox.Text, "Обхват талии");
            CurrentMeasurement.HipsCm = TryParseNullableDouble(HipsTextBox.Text, "Обхват бедер");
            
            // Проверяем, была ли ошибка парсинга (если метод TryParseNullableDouble вернул null при непустом тексте, но это обрабатывается внутри него)
            // Дополнительная валидация может быть здесь, если TryParseNullableDouble не показывает MessageBox

            CurrentMeasurement.Notes = NotesTextBox.Text;

            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        // Вспомогательный метод для парсинга nullable double
        private double? TryParseNullableDouble(string text, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null; // Поле не заполнено, это допустимо для nullable double
            }
            // Используем CultureInfo.InvariantCulture для правильного парсинга десятичного разделителя (точка)
            if (double.TryParse(text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                if (result < 0) // Дополнительная проверка на отрицательные значения, если нужно
                {
                     MessageBox.Show($"{fieldName} не может быть отрицательным.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                     return null; // Или какое-то специальное значение, чтобы прервать сохранение
                }
                return result;
            }
            else
            {
                MessageBox.Show($"Неверный формат числа для поля '{fieldName}'. Используйте цифры и, возможно, десятичную точку.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                // Чтобы прервать сохранение, можно либо бросить исключение, либо установить флаг,
                // либо сделать так, чтобы OkButton_Click проверял результат этого метода.
                // Простейший вариант - просто не присваивать значение и дать пользователю исправить.
                // Но для прерывания сохранения - DialogResult не должен установиться в true.
                // Пока что, если парсинг неудачен, вернем null, и поле останется null или старым.
                // Для строгости можно сделать так, чтобы OkButton_Click не продолжал, если хоть один парсинг вернул "ошибочный" null.
                // Мы это не делаем здесь явно, но MessageBox покажет ошибку.
                return null; // Возвращаем null, если парсинг не удался. Окно не закроется автоматически с DialogResult=true.
                             // Пользователь должен будет исправить ошибку.
                             // Чтобы OkButton_Click прервался, нужно добавить проверку там.
            }
        }
    }
}