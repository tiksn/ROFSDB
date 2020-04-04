using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TIKSN.ROFSDB.Serialization
{
    public interface ISerialization
    {
        IEnumerable<string> FileExtensions { get; }

        IAsyncEnumerable<T> GetDocumentsAsync<T>(Stream stream, CancellationToken cancellationToken);
    }
}