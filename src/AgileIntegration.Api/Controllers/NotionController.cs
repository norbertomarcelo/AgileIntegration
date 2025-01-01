using AgileIntegration.Modules.Dtos;
using AgileIntegration.Modules.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgileIntegration.Api.Controllers;

[Route("[controller]")]
public class NotionController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotionController> _logger;

    public NotionController(
        IConfiguration configuration,
        ILogger<NotionController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("/row")]
    public IActionResult CreateRow([FromBody] CreateTaskInput input)
    {
        var service = new NotionService(
            _configuration.GetValue<string>("Notion:PersonalAccessToken"),
            _configuration.GetValue<string>("Notion:DatabaseId"));

        try
        {
            var exists = service.CreateTask(input);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Problem();
        }
    }
}
