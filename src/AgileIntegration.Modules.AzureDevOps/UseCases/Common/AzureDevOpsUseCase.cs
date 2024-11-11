namespace AgileIntegration.Modules.AzureDevOps.UseCases.Common;

public abstract class AzureDevOpsUseCase
{
    protected readonly string _organization;
    protected readonly string _personalAccessToken;
    protected readonly string _project;
    protected readonly string _url;

    public AzureDevOpsUseCase(
        string organization,
        string personalAccessToken,
        string project,
        string url)
    {
        _organization = organization;
        _personalAccessToken = personalAccessToken;
        _project = project;
        _url = url;
    }
}
