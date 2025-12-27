namespace BrainstormingApp.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when user is not authorized to perform an action
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException() : base("You are not authorized to perform this action")
    {
    }

    public UnauthorizedException(string message) : base(message)
    {
    }
}
