using Storage.Net;
using Storage.Net.Blobs;
using System.Text;
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

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("---");
            stringBuilder.AppendLine("ID: 1100746772");
            stringBuilder.AppendLine("Name: United States");
            stringBuilder.AppendLine("---");
            stringBuilder.AppendLine("ID: 965475701");
            stringBuilder.AppendLine("Name: Canada");
            stringBuilder.AppendLine("...");
            await blobStorage.WriteTextAsync("/Countries/NorthAmerica.yaml", stringBuilder.ToString(), Encoding.UTF8);

            stringBuilder.Clear();
            stringBuilder.AppendLine("---");
            stringBuilder.AppendLine("ID: 1419150635");
            stringBuilder.AppendLine("Name: Austria");
            stringBuilder.AppendLine("---");
            stringBuilder.AppendLine("ID: 1552721979");
            stringBuilder.AppendLine("Name: France");
            stringBuilder.AppendLine("---");
            stringBuilder.AppendLine("ID: 1501801186");
            stringBuilder.AppendLine("Name: Italy");
            stringBuilder.AppendLine("...");
            await blobStorage.WriteTextAsync("/Countries/Europe.yaml", stringBuilder.ToString(), Encoding.UTF8);

            stringBuilder.Clear();
            stringBuilder.AppendLine("---");
            stringBuilder.AppendLine("ID: 918909193");
            stringBuilder.AppendLine("Name: New York City");
            stringBuilder.AppendLine("CountryID: 1100746772");
            stringBuilder.AppendLine("...");
            await blobStorage.WriteTextAsync("/Cities/Megacities.yaml", stringBuilder.ToString(), Encoding.UTF8);

            stringBuilder.Clear();
            stringBuilder.AppendLine("---");
            stringBuilder.AppendLine("ID: 356389956");
            stringBuilder.AppendLine("Name: Austin");
            stringBuilder.AppendLine("CountryID: 1100746772");
            stringBuilder.AppendLine("---");
            stringBuilder.AppendLine("ID: 1572248850");
            stringBuilder.AppendLine("Name: Toronto");
            stringBuilder.AppendLine("CountryID: 965475701");
            stringBuilder.AppendLine("---");
            stringBuilder.AppendLine("ID: 1859443008");
            stringBuilder.AppendLine("Name: Vienna");
            stringBuilder.AppendLine("CountryID: 1419150635");
            stringBuilder.AppendLine("---");
            stringBuilder.AppendLine("ID: 1948404451");
            stringBuilder.AppendLine("Name: Paris");
            stringBuilder.AppendLine("CountryID: 1552721979");
            stringBuilder.AppendLine("ID: 1062005753");
            stringBuilder.AppendLine("Name: Rome");
            stringBuilder.AppendLine("CountryID: 1501801186");
            stringBuilder.AppendLine("...");
            await blobStorage.WriteTextAsync("/Cities/Non-Megacities.yaml", stringBuilder.ToString(), Encoding.UTF8);
            return blobStorage;
        }
    }
}