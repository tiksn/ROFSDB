using System;

namespace TIKSN.ROFSDB
{
    public class FileStorageEntry
    {
        public FileStorageEntry(bool isDirectory, string name)
        {
            IsDirectory = isDirectory;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public bool IsDirectory { get; }
        public string Name { get; }
    }
}