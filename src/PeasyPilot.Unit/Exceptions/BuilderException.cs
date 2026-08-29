namespace PeasyPilot.Unit.Exceptions;

/// <summary>
/// Exception thrown when a builder configuration fails.
/// </summary>
public class BuilderException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BuilderException"/> class.
    /// </summary>
    public BuilderException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BuilderException"/> class with a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public BuilderException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BuilderException"/> class with a message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public BuilderException(string message, Exception innerException) 
        : base(message, innerException) { }
}
