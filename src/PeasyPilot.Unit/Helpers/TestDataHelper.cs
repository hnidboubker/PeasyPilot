namespace PeasyPilot.Unit.Helpers;

/// <summary>
/// Helper methods for unit testing operations.
/// </summary>
public static class TestDataHelper
{
    /// <summary>
    /// Validates that an object is not null and meets basic criteria.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValidTestObject<T>(T? obj) where T : class
    {
        return obj != null;
    }

    /// <summary>
    /// Creates a deep copy of the given object using reflection.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object to copy.</param>
    /// <returns>A shallow copy of the object.</returns>
    public static T ShallowCopy<T>(T obj) where T : class
    {
        var type = obj.GetType();
        if (type.IsValueType)
            return obj;

        var copy = Activator.CreateInstance(type) as T;
        if (copy == null)
            throw new InvalidOperationException($"Failed to create copy of {typeof(T).Name}");

        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            if (property.CanRead && property.CanWrite)
            {
                var value = property.GetValue(obj);
                property.SetValue(copy, value);
            }
        }

        return copy;
    }
}
