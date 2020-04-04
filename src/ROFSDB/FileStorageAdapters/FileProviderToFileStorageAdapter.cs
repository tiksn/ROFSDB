using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Text;

namespace TIKSN.ROFSDB.FileStorageAdapters
{
    public class FileProviderToFileStorageAdapter : IFileStorage
    {
        private readonly IFileProvider fileProvider;

        public FileProviderToFileStorageAdapter(IFileProvider fileProvider)
        {
            this.fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        }
    }
}
