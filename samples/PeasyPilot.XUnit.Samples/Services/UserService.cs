using PeasyPilot.XUnit.Samples.Models;

namespace PeasyPilot.XUnit.Samples.Services;

/// <summary>
/// Sample service for user operations.
/// </summary>
public class UserService
{
    private readonly List<User> _users = new();

    public User Create(User user)
    {
        user.Id = _users.Count + 1;
        user.CreatedAt = DateTime.UtcNow;
        _users.Add(user);
        return user;
    }

    public User? GetById(int? id) => _users.FirstOrDefault(u => u.Id == id);

    
    public List<User> GetAll() => _users.ToList();

    public bool Delete(int id)
    {
        var user = GetById(id);
        if (user == null) return false;
        _users.Remove(user);
        return true;
    }
}
