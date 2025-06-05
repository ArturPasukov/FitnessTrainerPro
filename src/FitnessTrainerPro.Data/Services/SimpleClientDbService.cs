// src/FitnessTrainerPro.Data/Services/SimpleClientDbService.cs
using FitnessTrainerPro.Core.Models;
using FitnessTrainerPro.Core.Services;
using FitnessTrainerPro.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitnessTrainerPro.Data.Services
{
    public class SimpleClientDbService : ISimpleClientDbService
    {
        private readonly FitnessDbContext _context;

        public SimpleClientDbService(FitnessDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddClientAsync(Client client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteClientAsync(int clientId)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client != null)
            {
                // Перед удалением клиента, возможно, нужно обработать связанные сущности,
                // если нет каскадного удаления или нужно выполнить доп. логику.
                // Например, ClientMeasurements, ClientAssignedPrograms.
                // Пока что просто удаляем клиента.
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Client>> GetAllClientsAsync()
        {
            return await _context.Clients
                                 .OrderBy(c => c.LastName)
                                 .ThenBy(c => c.FirstName)
                                 .ToListAsync();
        }

        public async Task<Client?> GetClientByIdAsync(int clientId)
        {
            // Для загрузки связанных данных (AssignedPrograms, Measurements) при получении клиента
            // можно использовать Include, если это нужно сразу.
            // return await _context.Clients
            //                      .Include(c => c.AssignedPrograms)
            //                      .Include(c => c.Measurements)
            //                      .FirstOrDefaultAsync(c => c.ClientID == clientId);
            return await _context.Clients.FindAsync(clientId);
        }

        public async Task UpdateClientAsync(Client client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            var existingClient = await _context.Clients.FindAsync(client.ClientID);
            if (existingClient != null)
            {
                existingClient.FirstName = client.FirstName;
                existingClient.LastName = client.LastName;
                existingClient.DateOfBirth = client.DateOfBirth;
                existingClient.PhoneNumber = client.PhoneNumber;
                existingClient.Email = client.Email;
                existingClient.Goals = client.Goals;
                // Не обновляем здесь коллекции AssignedPrograms и Measurements,
                // так как это сложная логика синхронизации.
                await _context.SaveChangesAsync();
            }
        }
    }
}