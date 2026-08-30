namespace PeasyPilot.Core.Exceptions;

/// <summary>
/// Exception thrown when a fixture initialization fails.
/// </summary>
public class FixtureException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FixtureException"/> class.
    /// </summary>
    public FixtureException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FixtureException"/> class with a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public FixtureException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FixtureException"/> class with a message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public FixtureException(string message, Exception innerException) 
        : base(message, innerException) { }
}
