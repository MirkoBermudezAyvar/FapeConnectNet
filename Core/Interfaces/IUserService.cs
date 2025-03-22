// Core/Interfaces/IUserService.cs
using venar_bus_api_jakar_bckd_net.Core.Entities;
using venar_bus_api_jakar_bckd_net.DTOs;

namespace venar_bus_api_jakar_bckd_net.Core.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(CreateUserDto userDto);
        Task UpdateUserAsync(int id, UpdateUserDto userDto);
        Task DeleteUserAsync(int id);
        Task<AuthResponseDto?> AuthenticateAsync(LoginDto loginDto);
    }
}