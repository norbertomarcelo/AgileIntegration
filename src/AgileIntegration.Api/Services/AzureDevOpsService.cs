using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace AgileIntegration.Api.Services;

public class AzureDevOpsService
{
    private readonly IConfiguration _configuration;
    private readonly string _organization;
    private readonly string _personalAccessToken;
    private readonly string _project;
    private readonly string _url;

    public AzureDevOpsService(IConfiguration configuration)
    {
        _configuration = configuration;
        _organization = _configuration.GetValue<string>("AzureDevOps:Organization");
        _personalAccessToken = _configuration.GetValue<string>("AzureDevOps:PersonalAccessToken");
        _project = _configuration.GetValue<string>("AzureDevOps:Project");
        _url = _configuration.GetValue<string>("AzureDevOps:Url");
    }

    public async Task<WorkItem> CreateWorkItem()
    {
        Uri uri = new Uri($"{_url}/{_organization}");
        VssBasicCredential credentials = new VssBasicCredential(string.Empty, _personalAccessToken);
        JsonPatchDocument document = new JsonPatchDocument();

        document.Add(
            new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = "/fields/System.Title",
                Value = "Test 03"
            }
        );

        VssConnection connection = new VssConnection(uri, credentials);
        WorkItemTrackingHttpClient workItemTrackingHttpClient = connection.GetClient<WorkItemTrackingHttpClient>();

        try
        {
            WorkItem result = await workItemTrackingHttpClient.CreateWorkItemAsync(document, _project, "Task");

            return result;
        }
        catch (AggregateException ex)
        {
            Console.WriteLine("Error creating bug: {0}", ex.InnerException.Message);
            return null;
        }
    }
}
