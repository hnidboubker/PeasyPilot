namespace PeasyPilot.XUnit.Samples.StepDefinitions;

using PeasyPilot.BDD.StepDefinitions;
using PeasyPilot.XUnit.Samples.Models;

/// <summary>
/// Step definitions for user management scenarios.
/// </summary>
public class UserSteps : BddStepDefinition
{
    private List<User> _users = new();
    private User? _currentUser;

    [Given("the user database is empty")]
    public void DatabaseEmpty()
    {
        _users.Clear();
    }

    [When("I create a user with email {email} and name {name}")]
    public void CreateUser(string email, string name)
    {
        var user = new User
        {
            Id = _users.Count + 1,
            Email = email,
            Name = name,
            CreatedAt = DateTime.UtcNow
        };
        _users.Add(user);
        _currentUser = user;
    }

    [Given("a user with email {email} and name {name}")]
    public void UserExists(string email, string name)
    {
        CreateUser(email, name);
    }

    [When("I retrieve the user")]
    public void RetrieveUser()
    {
        if (_currentUser == null && _users.Any())
        {
            _currentUser = _users.Last();
        }
    }

    [Then("the user should exist in the database")]
    public bool UserExists()
    {
        return _currentUser != null && _users.Contains(_currentUser);
    }

    [Then("the user count should be {count:int}")]
    public bool UserCountIs(int count)
    {
        return _users.Count == count;
    }

    [Then("the user name should be {name}")]
    public bool UserNameIs(string name)
    {
        return _currentUser?.Name == name;
    }

    [Then("the user email should be {email}")]
    public bool UserEmailIs(string email)
    {
        return _currentUser?.Email == email;
    }

    [When("I create a user with email {email} and name {name}")]
    public void CreateAnotherUser(string email, string name)
    {
        CreateUser(email, name);
    }

    [Then("the database should contain a user named {name}")]
    public bool DatabaseContainsUserNamed(string name)
    {
        return _users.Any(u => u.Name == name);
    }

    [When("I delete the user")]
    public void DeleteUser()
    {
        if (_currentUser != null)
        {
            _users.Remove(_currentUser);
        }
    }

    [Then("the user should not exist in the database")]
    public bool UserNotExists()
    {
        return _currentUser == null || !_users.Contains(_currentUser);
    }
}
