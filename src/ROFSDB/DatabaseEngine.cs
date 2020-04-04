using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TIKSN.ROFSDB
{
    public class DatabaseEngine : IDatabaseEngine
    {
        private readonly IFileStorage fileStorage;

        public DatabaseEngine(IFileStorage fileStorage)
        {
            this.fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        }

        public Task<string[]> GetCollectionsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<T> GetDocumentsAsync<T>(string collectionName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}