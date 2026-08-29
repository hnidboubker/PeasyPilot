namespace PeasyPilot.XUnit.Samples;

using Xunit;
using PeasyPilot.XUnit;
using PeasyPilot.Bogus;

/// <summary>
/// Sample domain model for testing.
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

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

    public User? GetById(int id) => _users.FirstOrDefault(u => u.Id == id);

    public List<User> GetAll() => _users.ToList();

    public bool Delete(int id)
    {
        var user = GetById(id);
        if (user == null) return false;
        _users.Remove(user);
        return true;
    }
}

/// <summary>
/// Sample xUnit tests demonstrating PeasyPilot usage.
/// </summary>
public class UserServiceXUnitTests : PeasyPilotTestBase
{
    private UserService _service = null!;
    private TestDataFactory _dataFactory = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new UserService();
        _dataFactory = new TestDataFactory();
    }

    [Fact]
    public void CreateUser_WithValidData_ReturnsCreatedUser()
    {
        // Arrange
        var user = new User { Name = "John Doe", Email = "john@example.com" };

        // Act
        var result = _service.Create(user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal("john@example.com", result.Email);
    }

    [Fact]
    public void GetById_WithValidId_ReturnsUser()
    {
        // Arrange
        var user = new User { Name = "Jane Smith", Email = "jane@example.com" };
        _service.Create(user);

        // Act
        var result = _service.GetById(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Jane Smith", result.Name);
    }

    [Fact]
    public void GetById_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = _service.GetById(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetAll_WithMultipleUsers_ReturnsAllUsers()
    {
        // Arrange
        _service.Create(new User { Name = "User 1", Email = "user1@example.com" });
        _service.Create(new User { Name = "User 2", Email = "user2@example.com" });
        _service.Create(new User { Name = "User 3", Email = "user3@example.com" });

        // Act
        var result = _service.GetAll();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Delete_WithValidId_RemovesUser()
    {
        // Arrange
        var user = new User { Name = "To Delete", Email = "delete@example.com" };
        _service.Create(user);

        // Act
        var deleted = _service.Delete(1);

        // Assert
        Assert.True(deleted);
        Assert.Null(_service.GetById(1));
    }

    [Theory]
    [InlineData("test1@example.com")]
    [InlineData("test2@example.com")]
    [InlineData("test3@example.com")]
    public void CreateUser_WithDifferentEmails_AllSucceed(string email)
    {
        // Arrange
        var user = new User { Name = "Test User", Email = email };

        // Act
        var result = _service.Create(user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
    }
}
