using AgileIntegration.Modules.Dtos;
using AgileIntegration.Modules.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgileIntegration.Api.Controllers;

[Route("[controller]")]
public class AzureDevOpsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureDevOpsController> _logger;

    public AzureDevOpsController(
        IConfiguration configuration,
        ILogger<AzureDevOpsController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("/task")]
    public async Task<IActionResult> CreateTask(
        [FromBody] CreateTaskInput input)
    {
        var service = new AzureDevOpsService(
            _configuration.GetValue<string>("AzureDevOps:Organization"),
            _configuration.GetValue<string>("AzureDevOps:Organization"),
            _configuration.GetValue<string>("AzureDevOps:Organization"),
            _configuration.GetValue<string>("AzureDevOps:PersonalAccessToken"),
            _configuration.GetValue<string>("AzureDevOps:Project"),
            _configuration.GetValue<string>("AzureDevOps:Url"));

        try
        {
            var output = await service.CreateTask(input);
            _logger.LogInformation(
                $"Work Item with title \"{output.Title}\" was created successfully.");
            return StatusCode(201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }
}
