namespace PeasyPilot.Core.Assertions;

/// <summary>
/// Exception thrown when an assertion fails in a test.
/// </summary>
public class AssertionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssertionException"/> class.
    /// </summary>
    public AssertionException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AssertionException"/> class with a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public AssertionException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AssertionException"/> class with a message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public AssertionException(string message, Exception innerException) : base(message, innerException) { }
}
