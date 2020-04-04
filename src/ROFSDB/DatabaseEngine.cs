using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TIKSN.ROFSDB
{
    public class DatabaseEngine : IDatabaseEngine
    {
        public Task<string[]> GetCollectionsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<T> GetDocumentsAsync<T>(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}