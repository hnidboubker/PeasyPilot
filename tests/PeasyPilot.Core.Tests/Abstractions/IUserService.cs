using PeasyPilot.Core.Tests.Models;

namespace PeasyPilot.Core.Tests.Abstractions;

public interface IUserService
{
    Task<int> CreateUserAsync(string name, string email);
    Task<E2EUser> GetUserAsync(int id);
    Task<IReadOnlyList<E2EUser>> GetAllUsersAsync();
}
