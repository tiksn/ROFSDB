using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Zio;

namespace TIKSN.ROFSDB.FileStorageAdapters
{
    public class ZioFileSystemToFileStorageAdapter : IFileStorage
    {
        private readonly IFileSystem fileSystem;

        public ZioFileSystemToFileStorageAdapter(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public async IAsyncEnumerable<FileStorageEntry> ListAsync(string fullPath, CancellationToken cancellationToken)
        {
            var items = fileSystem.EnumerateItems(fullPath, SearchOption.TopDirectoryOnly);

            foreach (var item in items)
            {
                yield return new FileStorageEntry(item.IsDirectory, item.GetName());
            }
        }

        public async Task<Stream> OpenReadAsync(string fullPath, CancellationToken cancellationToken)
        {
            FileEntry fileEntry = fileSystem.GetFileEntry(fullPath);
            return fileEntry.Open(FileMode.Open, FileAccess.Read);
        }
    }
}