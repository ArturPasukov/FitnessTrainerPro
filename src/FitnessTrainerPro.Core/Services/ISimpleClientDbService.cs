// src/FitnessTrainerPro.Core/Services/ISimpleClientDbService.cs
using FitnessTrainerPro.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitnessTrainerPro.Core.Services
{
    public interface ISimpleClientDbService
    {
        Task AddClientAsync(Client client);
        Task<Client?> GetClientByIdAsync(int clientId);
        Task<List<Client>> GetAllClientsAsync();
        Task UpdateClientAsync(Client client);
        Task DeleteClientAsync(int clientId);
        // Можно добавить и другие методы, например, для поиска клиентов
    }
}