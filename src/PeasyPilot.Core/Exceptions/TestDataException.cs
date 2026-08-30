namespace PeasyPilot.Core.Exceptions;

/// <summary>
/// Exception thrown when test data generation fails.
/// </summary>
public class TestDataException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestDataException"/> class.
    /// </summary>
    public TestDataException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestDataException"/> class with a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public TestDataException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestDataException"/> class with a message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public TestDataException(string message, Exception innerException) 
        : base(message, innerException) { }
}
