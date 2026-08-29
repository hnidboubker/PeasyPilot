using PeasyPilot.Core.Abstractions;
using Moq;
using System;

namespace PeasyPilot.Moq;

/// <summary>
/// Factory for creating mock objects using Moq.
/// </summary>
public class MockFactory : IMockFactory
{
    /// <summary>
    /// Creates a mock instance of the specified type.
    /// </summary>
    /// <param name="type">The type to mock.</param>
    /// <returns>A mock object.</returns>
    public object Create(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        var mockType = typeof(Mock<>).MakeGenericType(type);
        var mockInstance = Activator.CreateInstance(mockType);
        // Cast to Mock<T> and access Object property
        var objectProperty = mockType.GetProperty("Object");
        return objectProperty!.GetValue(mockInstance)!;
    }
}