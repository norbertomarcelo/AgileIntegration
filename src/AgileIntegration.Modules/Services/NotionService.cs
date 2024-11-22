using AgileIntegration.Modules.Dtos;
using AgileIntegration.Modules.Interfaces;
using Newtonsoft.Json.Linq;
using Notion.Client;
using System.Text;

namespace AgileIntegration.Modules.Services;

public class NotionService : ICommonServices
{
    private readonly string _personalAccessToken;
    private readonly string _databaseId;

    public NotionService(
        string personalAccessToken,
        string databaseId)
    {
        _personalAccessToken = personalAccessToken;
        _databaseId = databaseId;
    }

    public async Task<CreateTaskOutput> CreateTask(CreateTaskInput input)
    {
        if (TaskAlreadyExists(input.Title).Result)
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

    public async Task<bool> TaskAlreadyExists(CreateTaskInput input)
    {
        var client = NotionClientFactory.Create(new ClientOptions
        {
            AuthToken = _personalAccessToken
        });

        var filter = new TitleFilter("Nome", equal: input.Title);
        var queryParams = new DatabasesQueryParameters { Filter = filter };
        var pages = await client.Databases.QueryAsync(_databaseId, queryParams);
        return pages.Results.Count > 0;
    }
}
