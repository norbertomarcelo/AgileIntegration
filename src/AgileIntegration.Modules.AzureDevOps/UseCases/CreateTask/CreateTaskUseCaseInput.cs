namespace AgileIntegration.Modules.AzureDevOps.UseCases.CreateTask;

public class CreateTaskUseCaseInput
{
    public string AreaPath { get; set; }
    public string TeamProject { get; set; }
    public string State { get; set; }
    public string Title { get; set; }
    public int Priority { get; set; }
}
