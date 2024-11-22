using AgileIntegration.Modules.Dtos;
using AgileIntegration.Modules.Enums;
using AgileIntegration.Modules.Interfaces;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;


namespace AgileIntegration.Modules.Services;

public class AzureDevOpsService : ICommonServices
{
    private readonly string _areaPath;
    private readonly string _organization;
    private readonly string _personalAccessToken;
    private readonly string _project;
    private readonly string _teamProject;
    private readonly string _url;

    public AzureDevOpsService(
        string areaPath,
        string organization,
        string personalAccessToken,
        string project,
        string teamProject,
        string url)
    {
        _areaPath = areaPath;
        _organization = organization;
        _personalAccessToken = personalAccessToken;
        _project = project;
        _teamProject = teamProject;
        _url = url;
    }

    public async Task<CreateTaskOutput> CreateTask(CreateTaskInput input)
    {
        Uri uri = new Uri($"{_url}/{_organization}");
        VssBasicCredential credentials = new VssBasicCredential(string.Empty, _personalAccessToken);
        VssConnection connection = new VssConnection(uri, credentials);
        WorkItemTrackingHttpClient workItemTrackingHttpClient = connection.GetClient<WorkItemTrackingHttpClient>();

        if (ValidateTask(input, out string errorMessage))
            throw new InvalidDataException(errorMessage);

        if (TaskAlreadyExists(input).Result)
            throw new ArgumentException(input.Title);

        JsonPatchDocument document = new JsonPatchDocument();

        document.Add(
            new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = "/fields/System.AreaPath",
                Value = _areaPath
            }
        );
        document.Add(
            new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = "/fields/System.TeamProject",
                Value = _teamProject
            }
        );
        document.Add(
            new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = "/fields/System.State",
                Value = input.Status
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

            return new CreateTaskOutput
            {
                Title = input.Title,
                Description = input.Description,
                Priority = input.Priority,
                Status = input.Status
            };
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task<bool> TaskAlreadyExists(CreateTaskInput input)
    {
        Uri uri = new Uri($"{_url}/{_organization}");
        VssBasicCredential credentials = new VssBasicCredential(string.Empty, _personalAccessToken);
        using var httpClient = new WorkItemTrackingHttpClient(uri, new VssCredentials(credentials));

        var wiql = new Wiql()
        {
            Query = "Select [Id] " +
                "From WorkItems " +
                "Where [System.TeamProject] = '" + _project + "' " +
                "And [System.Title] = '" + input.Title + "' ",
        };

        var workItemList = await httpClient.QueryByWiqlAsync(wiql);
        return workItemList.WorkItems.Count() > 0;
    }

    public bool ValidateTask(CreateTaskInput input, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(input.Title))
        {
            errorMessage = "The Title is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(input.Description))
        {
            errorMessage = "The Description is required.";
            return false;
        }

        if (!Enum.IsDefined(typeof(Priority), input.Priority))
        {
            errorMessage = "The Priority is invalid.";
            return false;
        }

        if (!Enum.IsDefined(typeof(Status), input.Status))
        {
            errorMessage = "The Status is invalid.";
            return false;
        }

        return true;
    }
}
