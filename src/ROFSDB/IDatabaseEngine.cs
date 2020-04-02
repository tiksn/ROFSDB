using System.Collections.Generic;
using System.Threading;

namespace TIKSN.ROFSDB
{
    public interface IDatabaseEngine
    {
        string[] GetCollectionsAsync(CancellationToken cancellationToken);

        IAsyncEnumerable<T> GetDocumentsAsync<T>(CancellationToken cancellationToken);
    }
}