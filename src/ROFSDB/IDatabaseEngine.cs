using System.Collections.Generic;
using System.Threading;

namespace TIKSN.ROFSDB
{
    public interface IDatabaseEngine
    {
        IAsyncEnumerable<string> GetCollectionsAsync(CancellationToken cancellationToken);

        IAsyncEnumerable<T> GetDocumentsAsync<T>(string collectionName, CancellationToken cancellationToken);
    }
}