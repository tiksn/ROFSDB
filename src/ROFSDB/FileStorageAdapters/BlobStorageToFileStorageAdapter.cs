using Storage.Net.Blobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TIKSN.ROFSDB.FileStorageAdapters
{
    public class BlobStorageToFileStorageAdapter : IFileStorage
    {
        private readonly IBlobStorage blobStorage;

        public BlobStorageToFileStorageAdapter(IBlobStorage blobStorage)
        {
            this.blobStorage = blobStorage ?? throw new ArgumentNullException(nameof(blobStorage));
        }

        public async IAsyncEnumerable<FileStorageEntry> ListAsync(string fullPath, CancellationToken cancellationToken)
        {
            var blobs = await blobStorage.ListAsync(new ListOptions
            {
                FolderPath = fullPath
            }, cancellationToken);

            foreach (var blob in blobs)
            {
                yield return new FileStorageEntry(blob.IsFolder, blob.Name);
            }
        }

        public Task<Stream> OpenReadAsync(string fullPath, CancellationToken cancellationToken)
        {
            return blobStorage.OpenReadAsync(fullPath);
        }
    }
}