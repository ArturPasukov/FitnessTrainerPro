using System;
using System.Globalization;
using System.Windows;
using FitnessTrainerPro.Core.Models;
using Microsoft.Win32; // Для OpenFileDialog
using System.IO;        // Для Path, File, Directory
using System.Windows.Media.Imaging; // Для BitmapImage

namespace FitnessTrainerPro.UI
{
    public partial class MeasurementEditWindow : Window
    {
        public ClientMeasurement CurrentMeasurement { get; private set; }
        private string _appPhotosFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClientPhotos");

        // Конструктор для нового замера
        public MeasurementEditWindow(int clientId)
        {
            InitializeComponent();
            CurrentMeasurement = new ClientMeasurement
            {
                ClientID = clientId,
                MeasurementDate = DateTime.Today
            };
            MeasurementDatePicker.SelectedDate = CurrentMeasurement.MeasurementDate;
            EnsurePhotosFolderExists();
        }

        // Конструктор для редактирования существующего замера
        public MeasurementEditWindow(ClientMeasurement measurementToEdit)
        {
            InitializeComponent();
            CurrentMeasurement = measurementToEdit;
            EnsurePhotosFolderExists();

            MeasurementDatePicker.SelectedDate = CurrentMeasurement.MeasurementDate;
            WeightTextBox.Text = CurrentMeasurement.WeightKg?.ToString(CultureInfo.InvariantCulture);
            ChestTextBox.Text = CurrentMeasurement.ChestCm?.ToString(CultureInfo.InvariantCulture);
            WaistTextBox.Text = CurrentMeasurement.WaistCm?.ToString(CultureInfo.InvariantCulture);
            HipsTextBox.Text = CurrentMeasurement.HipsCm?.ToString(CultureInfo.InvariantCulture);
            NotesTextBox.Text = CurrentMeasurement.Notes;

            // Загрузка и отображение существующих фото
            LoadAndDisplayPhoto(CurrentMeasurement.PhotoBeforePath, PhotoBeforePreviewImage, PhotoBeforePathTextBlock);
            LoadAndDisplayPhoto(CurrentMeasurement.PhotoAfterPath, PhotoAfterPreviewImage, PhotoAfterPathTextBlock);
        }

        private void EnsurePhotosFolderExists()
        {
            if (!Directory.Exists(_appPhotosFolder))
            {
                Directory.CreateDirectory(_appPhotosFolder);
            }
            // Можно также создавать подпапки для каждого клиента, например:
            // string clientSpecificFolder = Path.Combine(_appPhotosFolder, $"Client_{CurrentMeasurement.ClientID}");
            // if (!Directory.Exists(clientSpecificFolder))
            // {
            //     Directory.CreateDirectory(clientSpecificFolder);
            // }
            // И сохранять фото туда. Пока для простоты используем общую папку.
        }

        private void BrowsePhotoBeforeButton_Click(object sender, RoutedEventArgs e)
        {
            string? newPhotoPath = SelectAndCopyPhoto(CurrentMeasurement.ClientID, "before");
            if (newPhotoPath != null)
            {
                CurrentMeasurement.PhotoBeforePath = newPhotoPath; // Сохраняем относительный путь
                LoadAndDisplayPhoto(CurrentMeasurement.PhotoBeforePath, PhotoBeforePreviewImage, PhotoBeforePathTextBlock);
            }
        }

        private void BrowsePhotoAfterButton_Click(object sender, RoutedEventArgs e)
        {
            string? newPhotoPath = SelectAndCopyPhoto(CurrentMeasurement.ClientID, "after");
            if (newPhotoPath != null)
            {
                CurrentMeasurement.PhotoAfterPath = newPhotoPath; // Сохраняем относительный путь
                LoadAndDisplayPhoto(CurrentMeasurement.PhotoAfterPath, PhotoAfterPreviewImage, PhotoAfterPathTextBlock);
            }
        }

        private string? SelectAndCopyPhoto(int clientId, string type) // type = "before" or "after"
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = $"Выберите фото '{type.ToUpper()}'",
                Filter = "Файлы изображений (*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string sourceFilePath = openFileDialog.FileName;
                    // Генерируем уникальное имя файла, чтобы избежать конфликтов
                    // и включаем ID клиента и тип фото для удобства идентификации
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    string extension = Path.GetExtension(sourceFilePath);
                    string newFileName = $"Client{clientId}_{type}_{timestamp}{extension}";
                    
                    string destinationFilePath = Path.Combine(_appPhotosFolder, newFileName);

                    File.Copy(sourceFilePath, destinationFilePath, true); // Копируем файл, перезаписывая, если существует

                    // Возвращаем относительный путь (имя файла), так как он будет храниться в папке приложения
                    return newFileName; 
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при копировании файла: {ex.Message}", "Ошибка файла", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            return null;
        }

        private void LoadAndDisplayPhoto(string? photoFileName, System.Windows.Controls.Image imageControl, System.Windows.Controls.TextBlock pathTextBlock)
        {
            if (!string.IsNullOrWhiteSpace(photoFileName))
            {
                string fullPath = Path.Combine(_appPhotosFolder, photoFileName);
                if (File.Exists(fullPath))
                {
                    try
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad; // Чтобы файл не был заблокирован
                        bitmap.EndInit();
                        imageControl.Source = bitmap;
                        pathTextBlock.Text = photoFileName; // Показываем только имя файла
                    }
                    catch (Exception ex)
                    {
                        imageControl.Source = null;
                        pathTextBlock.Text = "Ошибка загрузки фото";
                        // MessageBox.Show($"Ошибка отображения фото: {ex.Message}", "Ошибка"); // Можно раскомментировать для отладки
                    }
                }
                else
                {
                    imageControl.Source = null;
                    pathTextBlock.Text = "Файл не найден";
                }
            }
            else
            {
                imageControl.Source = null;
                pathTextBlock.Text = string.Empty;
            }
        }


        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (MeasurementDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите дату замера.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CurrentMeasurement.MeasurementDate = MeasurementDatePicker.SelectedDate.Value;
            
            bool parsingError = false;
            CurrentMeasurement.WeightKg = TryParseNullableDouble(WeightTextBox.Text, "Вес", ref parsingError);
            CurrentMeasurement.ChestCm = TryParseNullableDouble(ChestTextBox.Text, "Обхват груди", ref parsingError);
            CurrentMeasurement.WaistCm = TryParseNullableDouble(WaistTextBox.Text, "Обхват талии", ref parsingError);
            CurrentMeasurement.HipsCm = TryParseNullableDouble(HipsTextBox.Text, "Обхват бедер", ref parsingError);

            if (parsingError) // Если была ошибка парсинга хотя бы одного числового поля, не закрываем окно
            {
                return; 
            }
            
            CurrentMeasurement.Notes = NotesTextBox.Text;
            // PhotoBeforePath и PhotoAfterPath уже установлены в CurrentMeasurement при выборе файлов

            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private double? TryParseNullableDouble(string text, string fieldName, ref bool errorOccurred)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null; 
            }
            if (double.TryParse(text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                if (result < 0) 
                {
                     MessageBox.Show($"{fieldName} не может быть отрицательным.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                     errorOccurred = true;
                     return null; 
                }
                return result;
            }
            else
            {
                MessageBox.Show($"Неверный формат числа для поля '{fieldName}'. Используйте цифры и, возможно, десятичную точку.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                errorOccurred = true;
                return null; 
            }
        }
    }
}