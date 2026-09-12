using PeasyPilot.Core.Tests.Abstractions;
using PeasyPilot.Core.Tests.Models;

namespace PeasyPilot.Core.Tests.Integration;

public class E2EUserService : IUserService
{
    private readonly IUserRepository _repository;

    public E2EUserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> CreateUserAsync(string name, string email)
    {
        var user = new E2EUser { Name = name, Email = email };
        await _repository.AddAsync(user);
        return user.Id;
    }

    public async Task<E2EUser> GetUserAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        return user ?? throw new InvalidOperationException($"User {id} not found");
    }

    public Task<IReadOnlyList<E2EUser>> GetAllUsersAsync()
    {
        return _repository.GetAllAsync();
    }
}
