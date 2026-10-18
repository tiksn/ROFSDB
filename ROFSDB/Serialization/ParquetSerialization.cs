using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Parquet;
using Parquet.Serialization;

namespace TIKSN.ROFSDB.Serialization;

public class ParquetSerialization : ISerialization
{
    private static readonly IEnumerable<string> fileExtensions = [".parquet"];
    private static readonly ParquetOptions parquetOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public IEnumerable<string> FileExtensions => fileExtensions;

    public async IAsyncEnumerable<T> GetDocumentsAsync<T>(
        Stream stream,
        [EnumeratorCancellation] CancellationToken cancellationToken)
        where T : class, new()
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await ParquetSerializer.DeserializeAsync<T>(
            stream,
            parquetOptions,
            cancellationToken: cancellationToken);

        foreach (var item in result.Data)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
        }
    }
}