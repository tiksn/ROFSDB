using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TIKSN.ROFSDB.FileStorageAdapters
{
    public class FileProviderToFileStorageAdapter : IFileStorage
    {
        private readonly IFileProvider fileProvider;

        public FileProviderToFileStorageAdapter(IFileProvider fileProvider)
        {
            this.fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        }

        public async IAsyncEnumerable<FileStorageEntry> ListAsync(string fullPath, CancellationToken cancellationToken)
        {
            foreach (var file in fileProvider.GetDirectoryContents(fullPath))
            {
                yield return new FileStorageEntry(file.IsDirectory, file.Name);
            }
        }

        public async Task<Stream> OpenReadAsync(string fullPath, CancellationToken cancellationToken)
        {
            var fileInfo = fileProvider.GetFileInfo(fullPath);
            return fileInfo.CreateReadStream();
        }
    }
}