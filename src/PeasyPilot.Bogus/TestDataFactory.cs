using Bogus;
using PeasyPilot.Core.Abstractions;

namespace PeasyPilot.Bogus;

public class TestDataFactory : ITestDataFactory
{
    public T Create<T>() where T : class
    {
        return new Faker<T>().Generate();
    }

    public IReadOnlyCollection<T> CreateMany<T>(int count) where T : class
    {
        var faker = new Faker<T>();
        return Enumerable.Range(0, count)
            .Select(_ => faker.Generate())
            .ToList();
    }
}
