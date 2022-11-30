using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TIKSN.ROFSDB.Serialization
{
    public interface ISerialization
    {
        IEnumerable<string> FileExtensions { get; }

        IAsyncEnumerable<T> GetDocumentsAsync<T>(
            Stream stream,
            [EnumeratorCancellation] CancellationToken cancellationToken)
            where T : class, new();
    }
}