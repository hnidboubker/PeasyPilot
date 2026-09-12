using PeasyPilot.Core.Tests.Models;

namespace PeasyPilot.Core.Tests.Abstractions;

public interface IUserRepository
{
    Task AddAsync(E2EUser user);
    Task<E2EUser?> GetByIdAsync(int id);
    Task<IReadOnlyList<E2EUser>> GetAllAsync();
}
