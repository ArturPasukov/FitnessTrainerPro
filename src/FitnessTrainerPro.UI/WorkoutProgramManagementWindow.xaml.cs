using System.Windows;
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic; // Для List и ToHashSet

namespace FitnessTrainerPro.UI
{
    public partial class WorkoutProgramManagementWindow : Window
    {
        public WorkoutProgramManagementWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPrograms();
        }

        private void LoadPrograms()
        {
            try
            {
                using (var dbContext = new FitnessDbContext())
                {
                    ProgramsListView.ItemsSource = dbContext.WorkoutPrograms.AsNoTracking().ToList(); // AsNoTracking для списка только для чтения
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка программ: {ex.ToString()}", "Ошибка загрузки", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddProgramButton_Click(object sender, RoutedEventArgs e)
        {
            WorkoutProgramEditWindow programEditWindow = new WorkoutProgramEditWindow();
            programEditWindow.Owner = this;

            if (programEditWindow.ShowDialog() == true)
            {
                WorkoutProgram newProgram = programEditWindow.CurrentProgram; // CurrentProgram уже содержит ProgramExercises
                try
                {
                    using (var dbContext = new FitnessDbContext())
                    {
                        // Для каждого ProgramExercise в новой программе, убедимся, что связанный Exercise
                        // не добавляется заново, если он уже есть в базе.
                        // Это актуально, если WorkoutProgramEditWindow заполняет ProgramExercise.Exercise
                        // (хотя мы решили делать ProgramExercise.Exercise = null там)
                        if (newProgram.ProgramExercises != null)
                        {
                            foreach (var pe in newProgram.ProgramExercises)
                            {
                                if (pe.Exercise != null && pe.ExerciseID == 0) // Если ExerciseID не установлен, но объект Exercise есть
                                {
                                     pe.ExerciseID = pe.Exercise.ExerciseID; // Убедимся, что ExerciseID взят из объекта
                                }
                                if (pe.ExerciseID == 0)
                                {
                                     MessageBox.Show($"Упражнение '{pe.Exercise?.Name ?? "Без имени"}' не имеет ID. Программа не будет сохранена корректно.", "Ошибка данных");
                                     return; // Прерываем сохранение
                                }
                                pe.Exercise = null; // Убираем ссылку на объект Exercise, чтобы EF Core связал по ExerciseID
                            }
                        }
                        
                        dbContext.WorkoutPrograms.Add(newProgram);
                        dbContext.SaveChanges();
                    }
                    LoadPrograms(); 
                }
                catch (DbUpdateException dbEx)
                {
                    string errorMessage = $"Ошибка добавления программы (DB): {dbEx.Message}";
                    if (dbEx.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутренняя ошибка БД: {dbEx.InnerException.Message}";
                    }
                    MessageBox.Show(errorMessage, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.Exception ex) 
                {
                    string errorMessage = $"Общая ошибка добавления программы: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                         if (ex.InnerException.InnerException != null)
                        {
                             errorMessage += $"\n\nВложенная внутренняя ошибка: {ex.InnerException.InnerException.Message}";
                        }
                    }
                    MessageBox.Show(errorMessage, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditProgramButton_Click(object sender, RoutedEventArgs e)
        {
            WorkoutProgram? selectedProgram = ProgramsListView.SelectedItem as WorkoutProgram;
            if (selectedProgram == null)
            {
                MessageBox.Show("Пожалуйста, выберите программу для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            WorkoutProgram? programWithDetails; // Этот объект будет передан в окно редактирования
            try
            {
                using (var dbContext = new FitnessDbContext())
                {
                    // Загружаем программу со всеми деталями для редактирования, но без отслеживания
                    programWithDetails = dbContext.WorkoutPrograms
                                                .Include(p => p.ProgramExercises) 
                                                    .ThenInclude(pe => pe.Exercise) 
                                                .AsNoTracking() 
                                                .FirstOrDefault(p => p.ProgramID == selectedProgram.ProgramID);
                }

                if (programWithDetails == null)
                {
                    MessageBox.Show("Выбранная программа не найдена в базе данных. Возможно, она была удалена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    LoadPrograms(); 
                    return;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки деталей программы: {ex.ToString()}", "Ошибка загрузки", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            WorkoutProgramEditWindow programEditWindow = new WorkoutProgramEditWindow(programWithDetails); 
            programEditWindow.Owner = this;

            if (programEditWindow.ShowDialog() == true)
            {
                // programEditWindow.CurrentProgram содержит программу с изменениями, включая измененный список ProgramExercises
                WorkoutProgram editedProgramFromDialog = programEditWindow.CurrentProgram; 
                try
                {
                    using (var dbContext = new FitnessDbContext())
                    {
                        // Загружаем существующую программу из базы для обновления (с отслеживанием)
                        var programInDb = dbContext.WorkoutPrograms
                                                 .Include(p => p.ProgramExercises)
                                                 .FirstOrDefault(p => p.ProgramID == editedProgramFromDialog.ProgramID);

                        if (programInDb != null)
                        {
                            // Обновляем основные поля самой программы
                            programInDb.Name = editedProgramFromDialog.Name;
                            programInDb.Description = editedProgramFromDialog.Description;
                            programInDb.Focus = editedProgramFromDialog.Focus;

                            // --- Начало логики синхронизации ProgramExercises ---
                            
                            // Список ProgramExerciseID из отредактированной программы (только для существующих)
                            var editedPeIds = editedProgramFromDialog.ProgramExercises
                                                .Where(pe => pe.ProgramExerciseID != 0)
                                                .Select(pe => pe.ProgramExerciseID)
                                                .ToHashSet();

                            // 1. Удаляем те ProgramExercise, которые есть в базе (programInDb.ProgramExercises),
                            // но отсутствуют в отредактированном списке (editedPeIds)
                            var exercisesInDbToRemove = new List<ProgramExercise>();
                            foreach (var dbPe in programInDb.ProgramExercises.ToList()) // ToList() для безопасного изменения коллекции
                            {
                                if (!editedPeIds.Contains(dbPe.ProgramExerciseID))
                                {
                                    exercisesInDbToRemove.Add(dbPe);
                                }
                            }
                            if (exercisesInDbToRemove.Any())
                            {
                                dbContext.ProgramExercises.RemoveRange(exercisesInDbToRemove);
                            }

                            // 2. Обновляем существующие и добавляем новые ProgramExercise
                            foreach (var editedPe in editedProgramFromDialog.ProgramExercises)
                            {
                                if (editedPe.ProgramExerciseID != 0) // Существующее упражнение (было в базе)
                                {
                                    var dbPe = programInDb.ProgramExercises.FirstOrDefault(p => p.ProgramExerciseID == editedPe.ProgramExerciseID);
                                    if (dbPe != null) // Оно должно быть найдено, если мы его не удалили выше
                                    {
                                        // Обновляем свойства существующего ProgramExercise
                                        dbPe.OrderInProgram = editedPe.OrderInProgram;
                                        dbPe.Sets = editedPe.Sets;
                                        dbPe.Reps = editedPe.Reps;
                                        dbPe.RestSeconds = editedPe.RestSeconds;
                                        dbPe.Notes = editedPe.Notes;
                                        // ExerciseID и ProgramID не должны меняться для существующей записи ProgramExercise
                                    }
                                }
                                else // Новое упражнение для программы (ProgramExerciseID == 0)
                                {
                                    // Убедимся, что ExerciseID корректен
                                    if (editedPe.ExerciseID == 0)
                                    {
                                        MessageBox.Show($"Ошибка: Упражнение '{editedPe.Exercise?.Name ?? "Без имени"}' в программе не имеет корректного ExerciseID и не может быть добавлено.", "Ошибка данных");
                                        continue; // Пропускаем это упражнение, не добавляем
                                    }
                                    
                                    // Устанавливаем навигационное свойство Exercise в null, чтобы EF Core связал по ExerciseID
                                    // и не пытался создать новый Exercise, если объект Exercise был передан.
                                    // Это уже должно быть сделано в WorkoutProgramEditWindow.
                                    editedPe.Exercise = null; 
                                    
                                    // Добавляем новый ProgramExercise в отслеживаемую коллекцию programInDb.ProgramExercises
                                    // EF Core автоматически установит ProgramID и пометит как Added.
                                    programInDb.ProgramExercises.Add(editedPe);
                                }
                            }
                            // --- Конец логики синхронизации ProgramExercises ---
                            
                            dbContext.SaveChanges();
                        }
                    }
                    LoadPrograms(); 
                }
                catch (DbUpdateException dbEx)
                {
                    string errorMessage = $"Ошибка редактирования программы (DB): {dbEx.Message}";
                    if (dbEx.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутренняя ошибка БД: {dbEx.InnerException.Message}";
                         if (dbEx.InnerException.InnerException != null)
                        {
                             errorMessage += $"\n\nВложенная внутренняя ошибка БД: {dbEx.InnerException.InnerException.Message}";
                        }
                    }
                    MessageBox.Show(errorMessage, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.Exception ex)
                {
                    string errorMessage = $"Общая ошибка редактирования программы: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                         if (ex.InnerException.InnerException != null)
                        {
                             errorMessage += $"\n\nВложенная внутренняя ошибка: {ex.InnerException.InnerException.Message}";
                        }
                    }
                    MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteProgramButton_Click(object sender, RoutedEventArgs e)
        {
            WorkoutProgram? selectedProgram = ProgramsListView.SelectedItem as WorkoutProgram;
            if (selectedProgram == null)
            {
                MessageBox.Show("Пожалуйста, выберите программу для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Вы уверены, что хотите удалить программу '{selectedProgram.Name}'?\nВНИМАНИЕ: Это также удалит все назначения этой программы клиентам и упражнения из этой программы.",
                                                     "Подтверждение удаления",
                                                     MessageBoxButton.YesNo,
                                                     MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var dbContext = new FitnessDbContext())
                    {
                        var programToDelete = dbContext.WorkoutPrograms
                                                     .Include(p => p.ProgramExercises) 
                                                     .Include(p => p.ClientAssignments) 
                                                     .FirstOrDefault(p => p.ProgramID == selectedProgram.ProgramID);

                        if (programToDelete != null)
                        {
                            // EF Core должен сам обработать удаление связанных ProgramExercises и ClientAssignments,
                            // если связи настроены как обязательные (не nullable FK) или с каскадным удалением.
                            // Для ProgramExercises -> ProgramID обязателен, так что они должны удалиться.
                            // Для ClientAssignments -> ProgramID обязателен, так что они тоже должны удалиться.
                            dbContext.WorkoutPrograms.Remove(programToDelete);
                            dbContext.SaveChanges();
                        }
                    }
                    LoadPrograms();
                }
                catch (DbUpdateException dbEx) 
                {
                    string errorMessage = $"Ошибка удаления программы (DB): {dbEx.Message}";
                    if (dbEx.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутренняя ошибка БД: {dbEx.InnerException.Message}";
                         if (dbEx.InnerException.InnerException != null)
                        {
                             errorMessage += $"\n\nВложенная внутренняя ошибка БД: {dbEx.InnerException.InnerException.Message}";
                        }
                    }
                    MessageBox.Show(errorMessage, "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.Exception ex)
                {
                     string errorMessage = $"Общая ошибка при удалении программы: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                         if (ex.InnerException.InnerException != null)
                        {
                             errorMessage += $"\n\nВложенная внутренняя ошибка: {ex.InnerException.InnerException.Message}";
                        }
                    }
                    MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}