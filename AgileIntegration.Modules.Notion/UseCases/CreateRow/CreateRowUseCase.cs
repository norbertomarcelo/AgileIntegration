using AgileIntegration.Modules.Notion.UseCases.Common;
using Newtonsoft.Json.Linq;
using System.Text;

namespace AgileIntegration.Modules.Notion.UseCases.CreateRow;

public class CreateRowUseCase : NotionUseCase
{
    public CreateRowUseCase(
        string personalAccessToken,
        string databaseId) : base(
            personalAccessToken,
            databaseId)
    { }

    public async Task Handle(CreateRowUseCaseInput input)
    {
        if (RowAlreadyExists(input.Name).Result)
            throw new Exception(
                $"There is already a card with the given title \"{input.Name}\"");

        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_personalAccessToken}");
        httpClient.DefaultRequestHeaders.Add("Notion-Version", "2022-06-28");

        var properties = new Dictionary<string, object>
        {
            ["Nome"] = new { title = new[] { new { text = new { content = "Novo Item" } } } },
        };

        var requestBody = new JObject
        {
            ["parent"] = new JObject { ["database_id"] = _databaseId },
            ["properties"] = JObject.FromObject(properties)
        };

        var content = new StringContent(
            requestBody.ToString(),
            Encoding.UTF8,
            "application/json"
        );

        var response = await httpClient.PostAsync(
            "https://api.notion.com/v1/pages",
            content
        );

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Erro ao adicionar item: {errorResponse}");
        }
    }
}
