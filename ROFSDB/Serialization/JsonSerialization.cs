using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;

namespace TIKSN.ROFSDB.Serialization;

public class JsonSerialization : ISerialization
{
    private static readonly IEnumerable<string> fileExtensions = [".json"];
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public IEnumerable<string> FileExtensions => fileExtensions;

    public async IAsyncEnumerable<T> GetDocumentsAsync<T>(
        Stream stream,
        [EnumeratorCancellation] CancellationToken cancellationToken)
        where T : class, new()
    {
        using var streamReader = new StreamReader(stream);
        var content = await streamReader.ReadToEndAsync(cancellationToken);

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in root.EnumerateArray())
            {
                var item = JsonSerializer.Deserialize<T>(element.GetRawText(), jsonOptions);
                if (item != null)
                {
                    yield return item;
                }
            }
        }
        else
        {
            var item = JsonSerializer.Deserialize<T>(root.GetRawText(), jsonOptions);
            if (item != null)
            {
                yield return item;
            }
        }
    }
}

