using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using TIKSN.ROFSDB.Serialization;

namespace TIKSN.ROFSDB;

public class DatabaseEngine : IDatabaseEngine
{
    private readonly IFileStorage fileStorage;
    private readonly FrozenDictionary<string, ISerialization> serializations;

    public DatabaseEngine(IFileStorage fileStorage, IServiceProvider serviceProvider)
    {
        this.fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        this.serializations = serviceProvider.GetKeyedServices<ISerialization>(KeyedService.AnyKey)
            .SelectMany(x => x.FileExtensions.Select(e => (fileExtension: e, serialization: x)))
            .ToFrozenDictionary(
            s => s.fileExtension,
            s => s.serialization,
            StringComparer.OrdinalIgnoreCase);
    }

    public IAsyncEnumerable<string> GetCollectionsAsync(CancellationToken cancellationToken)
    {
        return fileStorage
            .ListAsync("/", cancellationToken)
            .Where(x => x.IsDirectory)
            .Select(x => x.Name);
    }

    public async IAsyncEnumerable<T> GetDocumentsAsync<T>(
        string collectionName,
        [EnumeratorCancellation] CancellationToken cancellationToken)
        where T : class, new()
    {
        var files = fileStorage
            .ListAsync($"/{collectionName}", cancellationToken)
            .Where(x => !x.IsDirectory);

        await foreach (var fileName in files.Select(x => x.Name))
        {
            if (serializations.TryGetValue(Path.GetExtension(fileName), out var serialization))
            {
                await using var stream = await fileStorage.OpenReadAsync($"/{collectionName}/{fileName}", cancellationToken);
                await foreach (var doc in serialization.GetDocumentsAsync<T>(stream, cancellationToken))
                {
                    yield return doc;
                }
            }
        }
    }
}