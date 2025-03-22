// Core/Interfaces/IClientService.cs
using venar_bus_api_jakar_bckd_net.Core.Entities;
using venar_bus_api_jakar_bckd_net.DTOs;

namespace venar_bus_api_jakar_bckd_net.Core.Interfaces
{
    public interface IClientService
    {
        Task<IEnumerable<Client>> GetAllClientsAsync();
        Task<Client?> GetClientByIdAsync(int id);
        Task<Client> CreateClientAsync(CreateClientDto clientDto);
        Task UpdateClientAsync(int id, UpdateClientDto clientDto);
        Task DeleteClientAsync(int id);
        Task<IEnumerable<Client>> SearchClientsAsync(string term);
        Task<IEnumerable<Order>> GetClientOrdersAsync(int clientId);
    }
}