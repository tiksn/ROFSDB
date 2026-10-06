using Shouldly;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TIKSN.ROFSDB.Serialization;
using TIKSN.ROFSDB.Tests.Fixtures;
using TIKSN.ROFSDB.Tests.Models;
using Xunit;

namespace TIKSN.ROFSDB.Tests.Serialization;

[Collection("Database Context collection")]
public class JsonLinesSerializationTests(DatabaseContextFixture fixture) : IClassFixture<DatabaseContextFixture>
{
    private readonly JsonLinesSerialization serialization = new();
    private readonly DatabaseContextFixture fixture = fixture;

    [Fact]
    public void FileExtensions_ShouldReturnJsonLinesExtensions()
    {
        var extensions = serialization.FileExtensions.ToArray();

        extensions.ShouldContain(".jsonl");
        extensions.ShouldContain(".ndjson");
        extensions.Length.ShouldBe(2);
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldReadMultipleDocumentsFromJsonlFile()
    {
        var countries = await fixture.DatabaseContexts["JSONL"]
            .GetCountriesAsync(CancellationToken.None)
            .ToArrayAsync();

        countries.Length.ShouldBe(2);
        countries.Select(c => c.Name).ShouldBe(["Austria", "France"]);
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldReadCitiesFromNdJsonFile()
    {
        var cities = await fixture.DatabaseContexts["NDJSON"]
            .GetCitiesAsync(CancellationToken.None)
            .ToArrayAsync();

        cities.Length.ShouldBe(3);
        cities.Select(c => c.Name).ShouldBe(["New York City", "Austin", "Toronto"]);
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldIgnoreBlankLinesAndMatchCaseInsensitiveProperties()
    {
        var countries = await ReadCountriesAsync("""
            {"id": 1419150635, "name": "Austria"}

            {"ID": 1552721979, "Name": "France"}
            """);

        countries.Length.ShouldBe(2);
        countries.Select(c => c.Name).ShouldBe(["Austria", "France"]);
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldThrowWithLineNumberOnInvalidJson()
    {
        var exception = await Should.ThrowAsync<FormatException>(async () =>
            await ReadCountriesAsync("""
                {"id": 1419150635, "name": "Austria"}
                {"id": "not-an-integer"}
                {"id": 1552721979, "name": "France"}
                """));

        exception.Message.ShouldContain("line 2");
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldRespectCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in serialization.GetDocumentsAsync<Country>(
                new MemoryStream(Encoding.UTF8.GetBytes("{\"id\": 1419150635, \"name\": \"Austria\"}")),
                cts.Token))
            {
            }
        });
    }

    private async Task<Country[]> ReadCountriesAsync(
        string content,
        CancellationToken cancellationToken = default)
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        return await serialization
            .GetDocumentsAsync<Country>(stream, cancellationToken)
            .ToArrayAsync(cancellationToken);
    }
}
