﻿using AgileIntegration.Modules.AzureDevOps.Exceptions;
using AgileIntegration.Modules.AzureDevOps.UseCases.CreateTask;
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
    public async Task<IActionResult> CreateWorkItem(
        [FromBody] CreateTaskUseCaseInput input)
    {
        var useCase = new CreateTaskUseCase(
            _configuration.GetValue<string>("AzureDevOps:Organization"),
            _configuration.GetValue<string>("AzureDevOps:PersonalAccessToken"),
            _configuration.GetValue<string>("AzureDevOps:Project"),
            _configuration.GetValue<string>("AzureDevOps:Url"));

        try
        {
            var output = await useCase.Handle(input);
            _logger.LogInformation(
                $"Work Item with title \"{output.Title}\" was created successfully. Check ID {output.Id}");
            return StatusCode(201);
        }
        catch (WorkItemAlreadyExistsException ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return Problem(ex.Message);
        }
    }
}