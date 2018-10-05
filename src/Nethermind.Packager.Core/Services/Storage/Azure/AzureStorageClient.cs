using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Nethermind.Packager.Core.Services.Storage.Azure
{
    public class AzureStorageClient : IStorageClient
    {
        private readonly HttpClient _client;
        private readonly IOptions<StorageOptions> _options;

        public AzureStorageClient(HttpClient client, 
            IOptions<StorageOptions> options)
        {
            _client = client;
            _options = options;
        }

        public async Task<string> GetContentAsync()
            => await _client.GetStringAsync(
                $"{_options.Value.Url}/{_options.Value.Directory}?restype=container&comp=list");
    }
}