namespace PeasyPilot.XUnit.Samples;

using PeasyPilot.Unit.Builders;
using PeasyPilot.XUnit.Samples.Models;

/// <summary>
/// Example builder for creating user test objects.
/// </summary>
public class UserBuilder : BuilderBase<User>
{
    public UserBuilder WithName(string name)
    {
        Instance.Name = name;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        Instance.Email = email;
        return this;
    }

    public UserBuilder WithId(int id)
    {
        Instance.Id = id;
        return this;
    }

    public new UserBuilder Reset()
    {
        base.Reset();
        return this;
    }
}
