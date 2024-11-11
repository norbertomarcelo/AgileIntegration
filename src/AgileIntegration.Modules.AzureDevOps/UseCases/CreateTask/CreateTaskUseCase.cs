using AgileIntegration.Modules.AzureDevOps.Exceptions;
using AgileIntegration.Modules.AzureDevOps.UseCases.Common;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace AgileIntegration.Modules.AzureDevOps.UseCases.CreateTask;

public class CreateTaskUseCase : AzureDevOpsUseCase
{
    public CreateTaskUseCase(
        string organization,
        string personalAccessToken,
        string project,
        string url) : base(
            organization,
            personalAccessToken,
            project,
            url)
    { }

    public async Task<CreateTaskUseCaseOutput> Handle(CreateTaskUseCaseInput input)
    {
        Uri uri = new Uri($"{_url}/{_organization}");
        VssBasicCredential credentials =
            new VssBasicCredential(string.Empty, _personalAccessToken);
        VssConnection connection = new VssConnection(uri, credentials);
        WorkItemTrackingHttpClient workItemTrackingHttpClient =
            connection.GetClient<WorkItemTrackingHttpClient>();

        if (WorkItemAlreadyExists(input.Title).Result)
            throw new WorkItemAlreadyExistsException(
                $"There is already a work item with the given title \"{input.Title}\"");

        JsonPatchDocument document = new JsonPatchDocument();

        document.Add(
            new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = "/fields/System.AreaPath",
                Value = input.AreaPath
            }
        );

        document.Add(
            new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = "/fields/System.TeamProject",
                Value = input.TeamProject
            }
        );

        document.Add(
            new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = "/fields/System.State",
                Value = input.State
            }
        );

        document.Add(
            new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = "/fields/System.Title",
                Value = input.Title
            }
        );

        document.Add(
            new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = "/fields/Microsoft.VSTS.Common.Priority",
                Value = input.Priority
            }
        );

        try
        {
            WorkItem result = await workItemTrackingHttpClient
                .CreateWorkItemAsync(document, _project, "Task");

            return new CreateTaskUseCaseOutput
            {
                Id = result.Id,
                Title = input.Title
            };
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    private async Task<bool> WorkItemAlreadyExists(string title)
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
                    "Where [Work Item Type] = 'Task' " +
                    "And [System.TeamProject] = '" + _project + "' " +
                    "And [System.Title] = '" + title + "' " +
                    "Order By [State] Asc, [Changed Date] Desc",
            };

            var workItemList = await httpClient.QueryByWiqlAsync(wiql);

            if (workItemList.WorkItems.Count() == 0) return false;
            else return true;
        }
    }
}
