using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TIKSN.ROFSDB.Serialization;

namespace TIKSN.ROFSDB
{
    public class DatabaseEngine : IDatabaseEngine
    {
        private readonly IFileStorage fileStorage;
        private readonly ISerialization serialization;

        public DatabaseEngine(IFileStorage fileStorage, ISerialization serialization)
        {
            this.fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            this.serialization = serialization ?? throw new ArgumentNullException(nameof(serialization));
        }

        public IAsyncEnumerable<string> GetCollectionsAsync(CancellationToken cancellationToken)
        {
            return fileStorage
                .ListAsync("/", cancellationToken)
                .Where(x => x.IsDirectory)
                .Select(x => x.Name);
        }

        public async IAsyncEnumerable<T> GetDocumentsAsync<T>(string collectionName, CancellationToken cancellationToken)
        {
            var files = fileStorage
                .ListAsync($"/{collectionName}", cancellationToken)
                .Where(x => !x.IsDirectory && serialization.FileExtensions.Any(e => x.Name.EndsWith(e, StringComparison.OrdinalIgnoreCase)));

            await foreach (var file in files)
            {
                using var stream = await fileStorage.OpenReadAsync($"/{collectionName}/{file.Name}", cancellationToken);
                await foreach (var doc in serialization.GetDocumentsAsync<T>(stream, cancellationToken))
                {
                    yield return doc;
                }
            }
        }
    }
}