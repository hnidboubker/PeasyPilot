using PeasyPilot.Core.Tests.Abstractions;
using PeasyPilot.Core.Tests.Models;

namespace PeasyPilot.Core.Tests.Integration;

public class E2EUserRepository : IUserRepository, IResettable
{
    private readonly List<E2EUser> _users = new();
    private int _nextId = 1;

    public Task AddAsync(E2EUser user)
    {
        user.Id = _nextId++;
        _users.Add(user);
        return Task.CompletedTask;
    }

    public Task<E2EUser?> GetByIdAsync(int id)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
    }

    public Task<IReadOnlyList<E2EUser>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<E2EUser>>(_users.AsReadOnly());
    }

    public Task ResetAsync()
    {
        _users.Clear();
        _nextId = 1;
        return Task.CompletedTask;
    }
}
