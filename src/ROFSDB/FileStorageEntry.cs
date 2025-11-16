using System;

namespace TIKSN.ROFSDB;

public class FileStorageEntry(bool isDirectory, string name)
{
    public bool IsDirectory { get; } = isDirectory;
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
}