using AgileIntegration.Modules.Dtos;
using AgileIntegration.Modules.Services;
using AgileIntegration.UnitTests.Common;
using FluentAssertions;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Moq;

namespace AgileIntegration.UnitTests.Modules.Services;

public class AzureDevOpsTest
{
    private readonly BaseFixture _fixture;
    private readonly AzureDevOpsService _service;
    private readonly Mock<WorkItemTrackingHttpClient> _httpClientMock;

    public AzureDevOpsTest()
    {
        string areaPath = "AreaPath";
        string organization = "Organization";
        string personalAccessToken = "PAT";
        string project = "Project";
        string teamProject = "TeamProject";
        string url = "http://dummy.url";

        _fixture = new BaseFixture();
        _httpClientMock = new Mock<WorkItemTrackingHttpClient>(
            new Uri("http://dummy.url"),
            new VssCredentials()
        );
        _service = new AzureDevOpsService(
            areaPath,
            organization,
            personalAccessToken,
            project,
            teamProject,
            url
        );
    }

    [Fact(DisplayName = nameof(ValidateTask_WithValidInput_ReturnsTrue))]
    [Trait("Service", "ValidateTask")]
    public void ValidateTask_WithValidInput_ReturnsTrue()
    {
        var input = new CreateTaskInput
        {
            Title = _fixture.GetValidTitle(),
            Description = _fixture.GetValidDescription(),
            Priority = _fixture.GetValidPriority(),
            Status = _fixture.GetValidStatus(),
        };

        var taskIsValid = _service.ValidateTask(input, out string errorMessage);

        taskIsValid.Should().BeTrue();
    }

    [Fact(DisplayName = nameof(ValidateTask_WithInvalidTitle_ReturnsFalse))]
    [Trait("Service", "ValidateTask")]
    public void ValidateTask_WithInvalidTitle_ReturnsFalse()
    {
        var input = new CreateTaskInput
        {
            Title = _fixture.GetInvalidTitle(),
            Description = _fixture.GetValidDescription(),
            Priority = _fixture.GetValidPriority(),
            Status = _fixture.GetValidStatus(),
        };

        var taskIsValid = _service.ValidateTask(input, out string errorMessage);

        taskIsValid.Should().BeFalse();
        errorMessage.Should().Be("The Title is invalid.");
    }

    [Fact(DisplayName = nameof(ValidateTask_WithInvalidDescription_ReturnsFalse))]
    [Trait("Service", "ValidateTask")]
    public void ValidateTask_WithInvalidDescription_ReturnsFalse()
    {
        var input = new CreateTaskInput
        {
            Title = _fixture.GetValidTitle(),
            Description = _fixture.GetInvalidDescription(),
            Priority = _fixture.GetValidPriority(),
            Status = _fixture.GetValidStatus(),
        };

        var taskIsValid = _service.ValidateTask(input, out string errorMessage);

        taskIsValid.Should().BeFalse();
        errorMessage.Should().Be("The Description is invalid.");
    }

    [Fact(DisplayName = nameof(ValidateTask_WithInvalidPriority_ReturnsFalse))]
    [Trait("Service", "ValidateTask")]
    public void ValidateTask_WithInvalidPriority_ReturnsFalse()
    {
        var input = new CreateTaskInput
        {
            Title = _fixture.GetValidTitle(),
            Description = _fixture.GetValidDescription(),
            Priority = _fixture.GetInvalidPriority(),
            Status = _fixture.GetValidStatus(),
        };

        var taskIsValid = _service.ValidateTask(input, out string errorMessage);

        taskIsValid.Should().BeFalse();
        errorMessage.Should().Be("The Priority is invalid.");
    }

    [Fact(DisplayName = nameof(ValidateTask_WithInvalidStatus_ReturnsFalse))]
    [Trait("Service", "ValidateTask")]
    public void ValidateTask_WithInvalidStatus_ReturnsFalse()
    {
        var input = new CreateTaskInput
        {
            Title = _fixture.GetValidTitle(),
            Description = _fixture.GetValidDescription(),
            Priority = _fixture.GetValidPriority(),
            Status = _fixture.GetInvalidStatus(),
        };

        var taskIsValid = _service.ValidateTask(input, out string errorMessage);

        taskIsValid.Should().BeFalse();
        errorMessage.Should().Be("The Status is invalid.");
    }

    [Fact(DisplayName = nameof(CreateTask_ShouldThrowInvalidDataException_WhenTaskIsInvalid))]
    [Trait("Service", "CreateTask")]
    public async Task CreateTask_ShouldThrowInvalidDataException_WhenTaskIsInvalid()
    {
        var input = new CreateTaskInput
        {
            Title = _fixture.GetInvalidTitle(),
            Description = _fixture.GetValidDescription(),
            Priority = _fixture.GetValidPriority(),
            Status = _fixture.GetValidStatus(),
        };

        await _service.Invoking(fn => fn.CreateTask(input))
            .Should().ThrowAsync<InvalidDataException>()
            .WithMessage("The Title is invalid.");
    }

    [Fact(DisplayName = nameof(CreateTask_ShouldThrowArgumentException_WhenTaskAlreadyExists))]
    [Trait("Service", "CreateTask")]
    public async Task CreateTask_ShouldThrowArgumentException_WhenTaskAlreadyExists()
    {
        var input = new CreateTaskInput
        {
            Title = _fixture.GetValidTitle(),
            Description = _fixture.GetValidDescription(),
            Priority = _fixture.GetValidPriority(),
            Status = _fixture.GetValidStatus(),
        };

        _httpClientMock
            .Setup(client => client.QueryByWiqlAsync(
                It.IsAny<Wiql>(),
                null,
                null,
                null,
                default))
            .ReturnsAsync(
            new WorkItemQueryResult
            {
                WorkItems = new[] { new WorkItemReference() }
            });

        await _service.Invoking(fn => fn.CreateTask(input))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage($"There is already a task registered with the title {input.Title}");
    }
}
