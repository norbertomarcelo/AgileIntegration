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
        var taskIsValid = ValidateTask(input);
        if (!taskIsValid.IsValid) throw new InvalidDataException(taskIsValid.ErrorMessage);

        var taskAlreadyExists = await TaskAlreadyExists(input);
        if (!taskAlreadyExists.IsValid) throw new ArgumentException(taskAlreadyExists.ErrorMessage);

        var stateValue = (input.Status == 1) ? "To Do"
            : (input.Status == 2) ? "Doing" : "Done";


        var uri = new Uri($"{_url}/{_organization}");
        var credentials = new VssBasicCredential(string.Empty, _personalAccessToken);
        var connection = new VssConnection(uri, credentials);
        var httpClient = connection.GetClient<WorkItemTrackingHttpClient>();
        var document = new JsonPatchDocument();

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
                Path = "/fields/System.Title",
                Value = input.Title
            }
        );
        document.Add(
            new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = "/fields/System.Description",
                Value = $"<div>{input.Description}</div>"
            }
        );
        document.Add(
            new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = "/fields/System.State",
                Value = stateValue
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
            await httpClient.CreateWorkItemAsync(document, _project, "Task");

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

    public async Task<ValidationOutput> TaskAlreadyExists(CreateTaskInput input)
    {
        var wiql = new Wiql()
        {
            Query = "Select [Id] " +
                "From WorkItems " +
                "Where [System.TeamProject] = '" + _project + "' " +
                "And [System.Title] = '" + input.Title + "' ",
        };

        var uri = new Uri($"{_url}/{_organization}");
        var credentials = new VssBasicCredential(string.Empty, _personalAccessToken);
        var connection = new VssConnection(uri, credentials);
        var httpClient = connection.GetClient<WorkItemTrackingHttpClient>();
        var workItemList = await httpClient.QueryByWiqlAsync(wiql);

        if (workItemList.WorkItems.Count() > 0)
        {
            return new ValidationOutput
            {
                IsValid = false,
                ErrorMessage = $"There is already a task registered with the title: {input.Title}"
            };
        }

        return new ValidationOutput { IsValid = true };
    }

    public ValidationOutput ValidateTask(CreateTaskInput input)
    {
        var isValid = true;
        var errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(input.Title)
            || input.Title.Length < 3
            || input.Title.Length > 116)
        {
            isValid = false;
            errorMessage += "The Title is invalid.\n";
        }

        if (string.IsNullOrWhiteSpace(input.Description)
            || input.Description.Length < 3
            || input.Description.Length > 512)
        {
            isValid = false;
            errorMessage += "The Description is invalid.\n";
        }

        if (!Enum.IsDefined(typeof(Priority), input.Priority))
        {
            isValid = false;
            errorMessage += "The Priority is invalid.\n";
        }

        if (!Enum.IsDefined(typeof(Status), input.Status))
        {
            isValid = false;
            errorMessage += "The Status is invalid.\n";
        }

        return new ValidationOutput
        {
            IsValid = isValid,
            ErrorMessage = errorMessage
        };
    }
}
