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
                MessageBox.Show($"Ошибка загрузки списка программ: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        dbContext.WorkoutPrograms.Add(newProgram);
                        dbContext.SaveChanges();
                    }
                    LoadPrograms(); 
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления программы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditProgramButton_Click(object sender, RoutedEventArgs e)
        {
            WorkoutProgram? selectedProgram = ProgramsListView.SelectedItem as WorkoutProgram; // ЗДЕСЬ '?' УЖЕ БЫЛ
            if (selectedProgram == null)
            {
                MessageBox.Show("Пожалуйста, выберите программу для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            WorkoutProgram? programWithDetails; // <--- ИЗМЕНЕНО: добавлен '?'
            try
            {
                using (var dbContext = new FitnessDbContext())
                {
                    programWithDetails = dbContext.WorkoutPrograms
                                                .Include(p => p.ProgramExercises) 
                                                    .ThenInclude(pe => pe.Exercise) 
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
                MessageBox.Show($"Ошибка загрузки деталей программы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // programWithDetails здесь точно не null, т.к. выше есть return
            WorkoutProgramEditWindow programEditWindow = new WorkoutProgramEditWindow(programWithDetails); 
            programEditWindow.Owner = this;

            if (programEditWindow.ShowDialog() == true)
            {
                try
                {
                    using (var dbContext = new FitnessDbContext())
                    {
                        // programWithDetails (а значит и programEditWindow.CurrentProgram, т.к. это один и тот же объект)
                        // уже содержит измененные данные.
                        // Нам нужно найти соответствующую запись в текущем контексте (programInDb)
                        // и обновить ее свойства.
                        var programInDb = dbContext.WorkoutPrograms
                                                 .Include(p => p.ProgramExercises) 
                                                 .FirstOrDefault(p => p.ProgramID == programWithDetails.ProgramID);

                        if (programInDb != null)
                        {
                            programInDb.Name = programEditWindow.CurrentProgram.Name;
                            programInDb.Description = programEditWindow.CurrentProgram.Description;
                            programInDb.Focus = programEditWindow.CurrentProgram.Focus;
                            
                            // TODO: Логика обновления ProgramExercises
                            
                            dbContext.SaveChanges();
                        }
                    }
                    LoadPrograms(); 
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка редактирования программы: {ex.Message}\n{ex.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteProgramButton_Click(object sender, RoutedEventArgs e)
        {
            WorkoutProgram? selectedProgram = ProgramsListView.SelectedItem as WorkoutProgram; // ЗДЕСЬ '?' УЖЕ БЫЛ
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
                        var programToDelete = dbContext.WorkoutPrograms.Find(selectedProgram.ProgramID);
                        if (programToDelete != null)
                        {
                            dbContext.WorkoutPrograms.Remove(programToDelete);
                            dbContext.SaveChanges();
                        }
                    }
                    LoadPrograms();
                }
                catch (DbUpdateException dbEx) 
                {
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