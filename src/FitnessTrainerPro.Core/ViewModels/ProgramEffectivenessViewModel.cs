// FitnessTrainerPro.Core/ViewModels/ProgramEffectivenessViewModel.cs
// или FitnessTrainerPro.UI/ViewModels/ProgramEffectivenessViewModel.cs

namespace FitnessTrainerPro.Core.ViewModels // или FitnessTrainerPro.UI.ViewModels
{
    public class ProgramEffectivenessViewModel
    {
        public string ProgramName { get; set; } = string.Empty;
        public int ClientCount { get; set; } // Количество клиентов, по которым есть данные для этой программы
        public double AvgWeightChangeKg { get; set; }
        public double AvgWeightChangePercent { get; set; }
    }
}