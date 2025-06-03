using System.Windows;
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore; // Для обработки исключений, если потребуется

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
                    // Загружаем список программ. Пока без включения связанных упражнений.
                    ProgramsListView.ItemsSource = dbContext.WorkoutPrograms.ToList();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка программ: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddProgramButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем окно для новой программы
            WorkoutProgramEditWindow programEditWindow = new WorkoutProgramEditWindow();
            programEditWindow.Owner = this;

            if (programEditWindow.ShowDialog() == true)
            {
                // Если пользователь нажал OK, получаем новую программу
                WorkoutProgram newProgram = programEditWindow.CurrentProgram;
                try
                {
                    using (var dbContext = new FitnessDbContext())
                    {
                        // Добавляем программу в базу. 
                        // Упражнения в программе (ProgramExercises) пока не обрабатываем здесь,
                        // они должны быть добавлены/обновлены в WorkoutProgramEditWindow и сохранены вместе с программой,
                        // если CurrentProgram.ProgramExercises будет содержать их.
                        // На данном этапе CurrentProgram.ProgramExercises будет пустым.
                        dbContext.WorkoutPrograms.Add(newProgram);
                        dbContext.SaveChanges();
                    }
                    LoadPrograms(); // Обновляем список
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления программы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

            // При редактировании нам нужно загрузить связанные ProgramExercises,
            // чтобы передать их в WorkoutProgramEditWindow.
            WorkoutProgram programWithDetails;
            try
            {
                using (var dbContext = new FitnessDbContext())
                {
                    // Загружаем программу вместе с ее упражнениями
                    programWithDetails = dbContext.WorkoutPrograms
                                                .Include(p => p.ProgramExercises) // Включаем связанные упражнения
                                                    .ThenInclude(pe => pe.Exercise) // И информацию о самих упражнениях (название и т.д.)
                                                .FirstOrDefault(p => p.ProgramID == selectedProgram.ProgramID);
                }

                if (programWithDetails == null)
                {
                    MessageBox.Show("Выбранная программа не найдена в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    LoadPrograms(); // Обновить список, если программа исчезла
                    return;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки деталей программы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            WorkoutProgramEditWindow programEditWindow = new WorkoutProgramEditWindow(programWithDetails);
            programEditWindow.Owner = this;

            if (programEditWindow.ShowDialog() == true)
            {
                // Пользователь нажал OK. programEditWindow.CurrentProgram содержит измененную программу
                // и, в будущем, измененный список ProgramExercises.
                try
                {
                    using (var dbContext = new FitnessDbContext())
                    {
                        // Находим программу в БД для обновления.
                        // EF Core отследит изменения в programWithDetails (который является CurrentProgram в окне редактирования)
                        // если он был загружен из этого же контекста.
                        // Но так как мы пересоздаем контекст, нужно будет либо Attach и пометить как Modified,
                        // либо загрузить снова и скопировать свойства.
                        // Для простоты пока будем считать, что WorkoutProgramEditWindow обновляет переданный объект,
                        // и мы его сохраняем.
                        // ВАЖНО: работа с ProgramExercises потребует более сложной логики сохранения.

                        var programInDb = dbContext.WorkoutPrograms
                                                 .Include(p => p.ProgramExercises) // Включаем существующие ProgramExercises
                                                 .FirstOrDefault(p => p.ProgramID == programWithDetails.ProgramID);

                        if (programInDb != null)
                        {
                            // Обновляем основные поля программы
                            programInDb.Name = programEditWindow.CurrentProgram.Name;
                            programInDb.Description = programEditWindow.CurrentProgram.Description;
                            programInDb.Focus = programEditWindow.CurrentProgram.Focus;

                            // TODO: Более сложная логика для обновления ProgramExercises:
                            // 1. Удалить ProgramExercises, которых больше нет в programEditWindow.CurrentProgram.ProgramExercises
                            // 2. Обновить существующие ProgramExercises
                            // 3. Добавить новые ProgramExercises
                            // Пока что эта логика не реализована, и ProgramExercises не будут корректно обновляться.
                            // Это будет следующий шаг после того, как мы реализуем UI для управления ими в WorkoutProgramEditWindow.

                            dbContext.SaveChanges();
                        }
                    }
                    LoadPrograms(); // Обновляем список
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка редактирования программы: {ex.Message}\n{ex.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

            MessageBoxResult result = MessageBox.Show($"Вы уверены, что хотите удалить программу '{selectedProgram.Name}'?\nВНИМАНИЕ: Это также удалит все назначения этой программы клиентам и упражнения из этой программы (если настроено каскадное удаление).",
                                                     "Подтверждение удаления",
                                                     MessageBoxButton.YesNo,
                                                     MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var dbContext = new FitnessDbContext())
                    {
                        // При удалении WorkoutProgram, связанные ProgramExercises и ClientAssignedPrograms
                        // также должны быть удалены, если настроено каскадное удаление в БД (EF Core по умолчанию это делает для обязательных связей).
                        var programToDelete = dbContext.WorkoutPrograms.Find(selectedProgram.ProgramID);
                        if (programToDelete != null)
                        {
                            dbContext.WorkoutPrograms.Remove(programToDelete);
                            dbContext.SaveChanges();
                        }
                    }
                    LoadPrograms(); // Обновляем список
                }
                catch (DbUpdateException dbEx) // Ловим специфичные ошибки БД
                {
                    // Можно проверить InnerException на наличие ограничений внешнего ключа,
                    // если каскадное удаление не настроено или не сработало.
                    MessageBox.Show($"Ошибка удаления программы из базы данных: {dbEx.Message}\nВозможно, программа используется и не может быть удалена.", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления программы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}