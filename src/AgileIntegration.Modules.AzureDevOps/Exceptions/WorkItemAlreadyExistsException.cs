namespace AgileIntegration.Modules.AzureDevOps.Exceptions;

public class WorkItemAlreadyExistsException : Exception
{
    public WorkItemAlreadyExistsException(string? message)
        : base(message)
    { }
}
