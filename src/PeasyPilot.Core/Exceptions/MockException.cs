namespace PeasyPilot.Core.Exceptions;

/// <summary>
/// Exception thrown when a mock fails to be created.
/// </summary>
public class MockException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MockException"/> class.
    /// </summary>
    public MockException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MockException"/> class with a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public MockException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MockException"/> class with a message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public MockException(string message, Exception innerException) 
        : base(message, innerException) { }
}
