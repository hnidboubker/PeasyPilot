using Bogus;
using PeasyPilot.Core.Abstractions;

namespace PeasyPilot.Bogus;

/// <summary>
/// Default test data factory using Bogus for generating random test data.
///
/// Important: Bogus only populates properties that are explicitly configured via RuleFor().
/// Properties without rules remain at their default values (empty string, 0, null, false).
///
/// To populate all properties, configure rules explicitly when creating instances:
///
/// var faker = new Faker&lt;User&gt;()
///     .RuleFor(u => u.Name, f => f.Person.FullName())
///     .RuleFor(u => u.Email, f => f.Internet.Email());
/// var user = faker.Generate();
/// </summary>
public class TestDataFactory : ITestDataFactory
{
    public T Create<T>() where T : class => new Faker<T>().Generate();

    public IReadOnlyCollection<T> CreateMany<T>(int count) where T : class
    {
        var faker = new Faker<T>();
        return Enumerable.Range(0, count).Select(_ => faker.Generate()).ToList();
    }
}
