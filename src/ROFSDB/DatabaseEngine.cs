using System;
using System.Collections.Generic;
using System.Threading;

namespace TIKSN.ROFSDB
{
    public class DatabaseEngine : IDatabaseEngine
    {
        public string[] GetCollectionsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<T> GetDocumentsAsync<T>(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}