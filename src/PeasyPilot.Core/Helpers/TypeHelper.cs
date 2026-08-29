namespace PeasyPilot.Core.Helpers;

/// <summary>
/// Helper utilities for type operations.
/// </summary>
public static class TypeHelper
{
    /// <summary>
    /// Determines if a type is a simple type (value type, string, etc.).
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a simple type; otherwise, false.</returns>
    public static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive 
            || type == typeof(string) 
            || type == typeof(decimal) 
            || type == typeof(DateTime) 
            || type == typeof(DateTimeOffset) 
            || type == typeof(TimeSpan) 
            || type == typeof(Guid);
    }

    /// <summary>
    /// Determines if a type is a collection type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a collection type; otherwise, false.</returns>
    public static bool IsCollectionType(Type type)
    {
        return type.IsGenericType 
            && (type.GetGenericTypeDefinition() == typeof(List<>)
                || type.GetGenericTypeDefinition() == typeof(IList<>)
                || type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                || type.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>)
                || type.GetGenericTypeDefinition() == typeof(ICollection<>));
    }

    /// <summary>
    /// Gets the element type from a collection type.
    /// </summary>
    /// <param name="type">The collection type.</param>
    /// <returns>The element type, or null if not a collection type.</returns>
    public static Type? GetCollectionElementType(Type type)
    {
        if (!IsCollectionType(type))
            return null;

        return type.GetGenericArguments().FirstOrDefault();
    }
}
