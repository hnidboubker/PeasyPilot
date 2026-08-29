using Bogus;
using PeasyPilot.Core.Abstractions;
using System.Reflection;

namespace PeasyPilot.Bogus;

public class TestDataFactory : ITestDataFactory
{
    public T Create<T>() where T : class
    {
        var fakerType = typeof(Faker<>).MakeGenericType(typeof(T));
        var fakerInstance = Activator.CreateInstance(fakerType);
        var generateMethod = fakerType.GetMethod("Generate", Type.EmptyTypes);

        var result = generateMethod?.Invoke(fakerInstance, null);
        return (T)(result ?? throw new InvalidOperationException($"Failed to generate {typeof(T).Name}"));
    }

    public IReadOnlyCollection<T> CreateMany<T>(int count) where T : class
    {
        var fakerType = typeof(Faker<>).MakeGenericType(typeof(T));
        var fakerInstance = Activator.CreateInstance(fakerType);
        var generateMethod = fakerType.GetMethod("Generate", new[] { typeof(int) });

        var result = generateMethod?.Invoke(fakerInstance, new object[] { count });
        return (IReadOnlyCollection<T>)(result as IEnumerable<T> ?? throw new InvalidOperationException($"Failed to generate {typeof(T).Name}s"))!.ToList();
    }
}
