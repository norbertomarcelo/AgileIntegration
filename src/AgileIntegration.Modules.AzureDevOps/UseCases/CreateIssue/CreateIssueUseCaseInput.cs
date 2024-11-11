namespace AgileIntegration.Modules.AzureDevOps.UseCases.CreateIssue;

public class CreateIssueUseCaseInput
{
    public string AreaPath { get; set; }
    public string TeamProject { get; set; }
    public string State { get; set; }
    public string Title { get; set; }
    public int Priority { get; set; }
}
