namespace BrainstormingApp.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
public class NotFoundException : Exception
{
    public string ResourceName { get; }
    public object? ResourceId { get; }

    public NotFoundException(string resourceName, object? resourceId = null)
        : base($"{resourceName} was not found" + (resourceId != null ? $" with ID: {resourceId}" : ""))
    {
        ResourceName = resourceName;
        ResourceId = resourceId;
    }

    public NotFoundException(string message) : base(message)
    {
        ResourceName = "Resource";
    }
}
