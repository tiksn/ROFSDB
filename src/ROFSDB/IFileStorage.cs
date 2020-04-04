using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TIKSN.ROFSDB
{
    public interface IFileStorage
    {
        IAsyncEnumerable<FileStorageEntry> ListAsync(string fullPath, CancellationToken cancellationToken);

        Task<Stream> OpenReadAsync(string fullPath, CancellationToken cancellationToken);
    }
}