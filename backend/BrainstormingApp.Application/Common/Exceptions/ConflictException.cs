namespace BrainstormingApp.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when there is a conflict (e.g., duplicate entry)
/// </summary>
public class ConflictException : Exception
{
    public string ResourceName { get; }

    public ConflictException(string message) : base(message)
    {
        ResourceName = "Resource";
    }

    public ConflictException(string resourceName, string message) : base(message)
    {
        ResourceName = resourceName;
    }
}
