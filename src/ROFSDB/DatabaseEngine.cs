using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TIKSN.ROFSDB
{
    public class DatabaseEngine : IDatabaseEngine
    {
        private readonly IFileStorage fileStorage;

        public DatabaseEngine(IFileStorage fileStorage)
        {
            this.fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        }

        public IAsyncEnumerable<string> GetCollectionsAsync(CancellationToken cancellationToken)
        {
            return fileStorage
                .ListAsync("/", cancellationToken)
                .Where(x => x.IsDirectory)
                .Select(x => x.Name);
        }

        public IAsyncEnumerable<T> GetDocumentsAsync<T>(string collectionName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}