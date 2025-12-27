namespace BrainstormingApp.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleException : Exception
{
    public string RuleName { get; }

    public BusinessRuleException(string message) : base(message)
    {
        RuleName = "BusinessRule";
    }

    public BusinessRuleException(string ruleName, string message) : base(message)
    {
        RuleName = ruleName;
    }
}
