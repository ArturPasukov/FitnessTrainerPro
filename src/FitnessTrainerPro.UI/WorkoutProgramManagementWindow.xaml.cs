using System.Windows;
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore; 

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
                    ProgramsListView.ItemsSource = dbContext.WorkoutPrograms.ToList();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка программ: {ex.ToString()}", "Ошибка загрузки", MessageBoxButton.OK, MessageBoxImage.Error); // Используем ToString() для большей информации здесь
            }
        }

        private void AddProgramButton_Click(object sender, RoutedEventArgs e)
        {
            WorkoutProgramEditWindow programEditWindow = new WorkoutProgramEditWindow();
            programEditWindow.Owner = this;

            if (programEditWindow.ShowDialog() == true)
            {
                WorkoutProgram newProgram = programEditWindow.CurrentProgram;
                try
                {
                    using (var dbContext = new FitnessDbContext())
                    {
                        // Перед добавлением, если ProgramExercises не пуст,
                        // установим состояние связанных Exercise как Unchanged, если они уже существуют
                        // Это нужно, если WorkoutProgramEditWindow заполняет навигационное свойство ProgramExercise.Exercise
                        if (newProgram.ProgramExercises != null)
                        {
                            foreach (var pe in newProgram.ProgramExercises)
                            {
                                if (pe.Exercise != null && pe.Exercise.ExerciseID != 0)
                                {
                                    // Убедимся, что EF Core не пытается создать существующее упражнение заново
                                    var entry = dbContext.Entry(pe.Exercise);
                                    if (entry.State == EntityState.Detached) // Если Exercise не отслеживается
                                    {
                                        dbContext.Exercises.Attach(pe.Exercise); // Начинаем отслеживать
                                    }
                                    // Если Exercise уже отслеживается, его состояние должно быть корректным (Unchanged)
                                    // Если оно Added - будет ошибка.
                                    // Проще всего было бы в WorkoutProgramEditWindow в newProgramExercise.Exercise присваивать null,
                                    // а здесь EF Core сам бы связал по pe.ExerciseID.
                                    // Мы это уже сделали в WorkoutProgramEditWindow (newProgramExercise.Exercise = null;), так что этот блок может быть не нужен,
                                    // но оставим его для демонстрации, если бы навигационное свойство было заполнено.
                                    // Для нашего текущего кода, где ProgramExercise.Exercise = null, этот блок не сделает ничего плохого.
                                }
                            }
                        }
                        dbContext.WorkoutPrograms.Add(newProgram);
                        dbContext.SaveChanges();
                    }
                    LoadPrograms(); 
                }
                catch (DbUpdateException dbEx) // Ловим специфичные ошибки обновления БД
                {
                    string errorMessage = $"Ошибка добавления программы (DB): {dbEx.Message}";
                    if (dbEx.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутренняя ошибка БД: {dbEx.InnerException.Message}";
                    }
                    MessageBox.Show(errorMessage, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.Exception ex) // Ловим все остальные ошибки
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

            WorkoutProgram? programWithDetails;
            try
            {
                using (var dbContext = new FitnessDbContext())
                {
                    programWithDetails = dbContext.WorkoutPrograms
                                                .Include(p => p.ProgramExercises) 
                                                    .ThenInclude(pe => pe.Exercise) 
                                                .AsNoTracking() // Загружаем для редактирования, но не отслеживаем
                                                .FirstOrDefault(p => p.ProgramID == selectedProgram.ProgramID);
                }

                if (programWithDetails == null)
                {
                    MessageBox.Show("Выбранная программа не найдена в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                WorkoutProgram editedProgramFromDialog = programEditWindow.CurrentProgram;
                try
                {
                    using (var dbContext = new FitnessDbContext())
                    {
                        // Обновление WorkoutProgram
                        dbContext.WorkoutPrograms.Update(editedProgramFromDialog); // Помечаем программу и ее ProgramExercises как измененные

                        // Логика синхронизации коллекции ProgramExercises (удаление, добавление, обновление)
                        // Это сложная часть, которую нужно будет реализовать более детально.
                        // Пока что .Update() попытается сохранить граф как есть.
                        // Если ProgramExercises были изменены (добавлены/удалены элементы в списке),
                        // то Update может не справиться корректно без дополнительной логики.

                        // Пример более явного управления:
                        var programInDb = dbContext.WorkoutPrograms
                                                 .Include(p => p.ProgramExercises)
                                                 .FirstOrDefault(p => p.ProgramID == editedProgramFromDialog.ProgramID);

                        if (programInDb != null)
                        {
                            // Обновляем основные поля
                            programInDb.Name = editedProgramFromDialog.Name;
                            programInDb.Description = editedProgramFromDialog.Description;
                            programInDb.Focus = editedProgramFromDialog.Focus;

                            // --- Начало сложной логики синхронизации ProgramExercises ---
                            // 1. Удаляем те, что были в базе, но нет в новом списке
                            var exercisesToRemove = programInDb.ProgramExercises
                                .Where(peDb => !editedProgramFromDialog.ProgramExercises.Any(peEdited => peEdited.ProgramExerciseID != 0 && peEdited.ProgramExerciseID == peDb.ProgramExerciseID))
                                .ToList();
                            foreach (var exToRemove in exercisesToRemove)
                            {
                                dbContext.ProgramExercises.Remove(exToRemove);
                            }

                            // 2. Обновляем существующие и добавляем новые
                            foreach (var editedPe in editedProgramFromDialog.ProgramExercises)
                            {
                                if (editedPe.ProgramExerciseID != 0) // Существующее упражнение
                                {
                                    var dbPe = programInDb.ProgramExercises.FirstOrDefault(pe => pe.ProgramExerciseID == editedPe.ProgramExerciseID);
                                    if (dbPe != null)
                                    {
                                        dbPe.OrderInProgram = editedPe.OrderInProgram;
                                        dbPe.Sets = editedPe.Sets;
                                        dbPe.Reps = editedPe.Reps;
                                        dbPe.RestSeconds = editedPe.RestSeconds;
                                        dbPe.Notes = editedPe.Notes;
                                        // ExerciseID не должен меняться для существующего ProgramExercise
                                    }
                                }
                                else // Новое упражнение для программы
                                {
                                    // Убедимся, что ExerciseID корректен и Exercise не добавляется заново
                                    if (editedPe.Exercise != null && editedPe.Exercise.ExerciseID != 0)
                                    {
                                       var entry = dbContext.Entry(editedPe.Exercise);
                                       if(entry.State == EntityState.Detached) dbContext.Exercises.Attach(editedPe.Exercise);
                                    }
                                    else if (editedPe.ExerciseID == 0 && editedPe.Exercise != null)
                                    {
                                        // Если ExerciseID 0, но объект Exercise есть, это ошибка
                                        throw new InvalidOperationException("Новое упражнение в программе не имеет корректного ExerciseID.");
                                    }
                                    editedPe.Exercise = null; // Отсоединяем объект Exercise, полагаемся на ExerciseID

                                    programInDb.ProgramExercises.Add(editedPe); // EF Core установит ProgramID
                                }
                            }
                            // --- Конец сложной логики синхронизации ProgramExercises ---
                            
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
                        // Важно: чтобы EF Core корректно обработал каскадное удаление или удалил зависимые сущности,
                        // сначала нужно загрузить программу со всеми зависимостями, которые мы хотим удалить явно,
                        // или убедиться, что каскадное удаление настроено на уровне БД/EF Core.
                        // Простое `Find` и `Remove` может не удалить ProgramExercises, если нет каскадного удаления.
                        var programToDelete = dbContext.WorkoutPrograms
                                                     .Include(p => p.ProgramExercises) // Включаем для явного удаления или для срабатывания каскада EF Core
                                                     .Include(p => p.ClientAssignments) // Аналогично
                                                     .FirstOrDefault(p => p.ProgramID == selectedProgram.ProgramID);

                        if (programToDelete != null)
                        {
                            // Если нет каскадного удаления на уровне БД, и мы хотим удалить связанные ProgramExercises:
                            // dbContext.ProgramExercises.RemoveRange(programToDelete.ProgramExercises);
                            // Если нет каскадного удаления на уровне БД, и мы хотим удалить связанные ClientAssignments:
                            // dbContext.ClientAssignedPrograms.RemoveRange(programToDelete.ClientAssignments);
                            
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