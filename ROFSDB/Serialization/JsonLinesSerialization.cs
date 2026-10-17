using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;

namespace TIKSN.ROFSDB.Serialization;

public class JsonLinesSerialization : ISerialization
{
    private static readonly IEnumerable<string> fileExtensions = [".jsonl", ".ndjson"];
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
        var lineNumber = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await streamReader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                yield break;
            }

            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            T item;

            try
            {
                item = JsonSerializer.Deserialize<T>(line, jsonOptions);
            }
            catch (JsonException ex)
            {
                throw new FormatException($"Unable to deserialize JSON on line {lineNumber}.", ex);
            }
            catch (NotSupportedException ex)
            {
                throw new FormatException($"Unable to deserialize JSON on line {lineNumber}.", ex);
            }

            if (item != null)
            {
                yield return item;
            }
        }
    }
}
