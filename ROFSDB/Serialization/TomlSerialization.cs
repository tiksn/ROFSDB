using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using Tomlyn;

namespace TIKSN.ROFSDB.Serialization;

public class TomlSerialization : ISerialization
{
    private static readonly IEnumerable<string> fileExtensions = [".toml"];
    private static readonly TomlSerializerOptions tomlOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public IEnumerable<string> FileExtensions => fileExtensions;

    public async IAsyncEnumerable<T> GetDocumentsAsync<T>(
        Stream stream,
        [EnumeratorCancellation] CancellationToken cancellationToken)
        where T : class, new()
    {
        using var streamReader = new StreamReader(stream);

        var content = await streamReader.ReadToEndAsync(cancellationToken);

        var document = TomlSerializer.Deserialize<T>(content, tomlOptions);
        if (document != null)
        {
            yield return document;
        }
    }
}