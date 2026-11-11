using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Common.Authentication;
using Common.Models;
using Common.Utilities;

namespace Common.Providers;

internal class SearchProvider
{
    private const int MaxResults = 5;

    private static SearchClient? _client;

    private static async Task<SearchClient> GetClientAsync()
    {
        if (_client is not null)
        {
            return _client;
        }

        string endpoint = await Authenticator.GetSecretAsync(AppSettings.SearchEndpointName);
        string queryKey = await Authenticator.GetSecretAsync(AppSettings.SearchQueryKeyName);

        _client = new SearchClient(new Uri(endpoint), AppSettings.SearchIndex, new AzureKeyCredential(queryKey));
        return _client;
    }

    internal async Task<List<ProcedureDocument>> SearchAsync(string query)
    {
        SearchClient client = await GetClientAsync();

        var options = new SearchOptions
        {
            Size = MaxResults,
            Select = { "title", "chunk" },
            VectorSearch = new VectorSearchOptions
            {
                Queries =
                {
                    new VectorizableTextQuery(query)
                    {
                        KNearestNeighborsCount = MaxResults,
                        Fields = { "text_vector" }
                    }
                }
            }
        };

        SearchResults<SearchDocument> results = await client.SearchAsync<SearchDocument>(query, options);

        var chunks = new List<ProcedureDocument>();
        await foreach (SearchResult<SearchDocument> result in results.GetResultsAsync())
        {
            chunks.Add(new ProcedureDocument
            {
                Name = result.Document["title"]?.ToString() ?? string.Empty,
                Url = string.Empty,
                Text = result.Document["chunk"]?.ToString() ?? string.Empty
            });
        }

        return chunks;
    }
}
