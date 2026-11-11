using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Common.Authentication;
using Common.Models;
using Common.Utilities;

namespace Common.Providers;

internal class StorageProvider
{
    private static BlobContainerClient? _container;

    private static async Task<BlobContainerClient> GetContainerAsync()
    {
        if (_container is not null)
        {
            return _container;
        }

        string connectionString = await Authenticator.GetSecretAsync(AppSettings.StorageConnectionStringName);
        _container = new BlobServiceClient(connectionString).GetBlobContainerClient(AppSettings.StorageContainer);
        return _container;
    }

    internal async Task<List<ProcedureDocument>> GetDocumentsAsync()
    {
        BlobContainerClient container = await GetContainerAsync();
        var documents = new List<ProcedureDocument>();

        await foreach (BlobItem blob in container.GetBlobsAsync())
        {
            if (!blob.Name.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            using var memory = new MemoryStream();
            await container.GetBlobClient(blob.Name).DownloadToAsync(memory);
            memory.Position = 0;

            documents.Add(new ProcedureDocument
            {
                Name = Path.GetFileName(blob.Name),
                Url = string.Empty,
                Text = WordTextExtractor.Extract(memory)
            });
        }

        return documents;
    }
}
