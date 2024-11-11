using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;

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

    protected async Task<bool> WorkItemAlreadyExists(string title)
    {
        Uri uri = new Uri($"{_url}/{_organization}");
        VssBasicCredential credentials =
            new VssBasicCredential(string.Empty, _personalAccessToken);

        using (var httpClient = new WorkItemTrackingHttpClient(uri, new VssCredentials(credentials)))
        {
            var wiql = new Wiql()
            {
                Query = "Select [Id] " +
                    "From WorkItems " +
                    "Where [System.TeamProject] = '" + _project + "' " +
                    "And [System.Title] = '" + title + "' ",
            };

            var workItemList = await httpClient.QueryByWiqlAsync(wiql);

            if (workItemList.WorkItems.Count() == 0) return false;
            else return true;
        }
    }
}
