using Notion.Client;

namespace AgileIntegration.Modules.Notion.UseCases.Common;

public class NotionUseCase
{
    protected readonly string _personalAccessToken;
    protected readonly string _databaseId;

    public NotionUseCase(
        string personalAccessToken,
        string databaseId)
    {
        _personalAccessToken = personalAccessToken;
        _databaseId = databaseId;
    }

    protected async Task<bool> RowAlreadyExists(string title)
    {
        var client = NotionClientFactory.Create(new ClientOptions
        {
            AuthToken = _personalAccessToken
        });

        var filter = new TitleFilter("Nome", equal: title);
        var queryParams = new DatabasesQueryParameters { Filter = filter };
        var pages = await client.Databases.QueryAsync(_databaseId, queryParams);
        return pages.Results.Count > 0;
    }
}
