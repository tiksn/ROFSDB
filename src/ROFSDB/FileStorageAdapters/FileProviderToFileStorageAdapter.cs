using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TIKSN.ROFSDB.FileStorageAdapters
{
    public class FileProviderToFileStorageAdapter(IFileProvider fileProvider) : IFileStorage
    {
        private readonly IFileProvider fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));

        public async IAsyncEnumerable<FileStorageEntry> ListAsync(string fullPath, [EnumeratorCancellation] CancellationToken cancellationToken)
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