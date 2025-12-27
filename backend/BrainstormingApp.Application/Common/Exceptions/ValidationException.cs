namespace BrainstormingApp.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : Exception
{
    public List<string> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new List<string> { message };
    }

    public ValidationException(List<string> errors)
        : base("One or more validation errors occurred")
    {
        Errors = errors;
    }

    public ValidationException(string message, List<string> errors) : base(message)
    {
        Errors = errors;
    }
}
