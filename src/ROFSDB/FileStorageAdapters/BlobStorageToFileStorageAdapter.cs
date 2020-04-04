using Storage.Net.Blobs;
using System;

namespace TIKSN.ROFSDB.FileStorageAdapters
{
    public class BlobStorageToFileStorageAdapter : IFileStorage
    {
        private readonly IBlobStorage blobStorage;

        public BlobStorageToFileStorageAdapter(IBlobStorage blobStorage)
        {
            this.blobStorage = blobStorage ?? throw new ArgumentNullException(nameof(blobStorage));
        }
    }
}