using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TIKSN.ROFSDB
{
    public interface IDatabaseEngine
    {
        Task<string[]> GetCollectionsAsync(CancellationToken cancellationToken);

        IAsyncEnumerable<T> GetDocumentsAsync<T>(string collectionName, CancellationToken cancellationToken);
    }
}