using Storage.Net;
using Storage.Net.Blobs;
using System.Threading.Tasks;
using TIKSN.ROFSDB.FileStorageAdapters;
using Xunit;

namespace TIKSN.ROFSDB.Tests.Fixtures
{
    public class YamlDatabaseEngineFixture : IAsyncLifetime
    {
        public IDatabaseEngine DatabaseEngine { get; private set; }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            IBlobStorage blobStorage = await CreateBlobStorageAsync();
            DatabaseEngine = new DatabaseEngine(new BlobStorageToFileStorageAdapter(blobStorage));
        }

        private async Task<IBlobStorage> CreateBlobStorageAsync()
        {
            var blobStorage = StorageFactory.Blobs.InMemory();

            return blobStorage;
        }
    }
}