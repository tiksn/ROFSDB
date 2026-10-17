using Shouldly;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TIKSN.ROFSDB.Serialization;
using TIKSN.ROFSDB.Tests.Fixtures;
using TIKSN.ROFSDB.Tests.Models;
using Xunit;

namespace TIKSN.ROFSDB.Tests.Serialization;

[Collection("Database Context collection")]
public class ParquetSerializationTests(DatabaseContextFixture fixture) : IClassFixture<DatabaseContextFixture>
{
    private readonly ParquetSerialization serialization = new();
    private readonly DatabaseContextFixture fixture = fixture;

    [Fact]
    public void FileExtensions_ShouldReturnParquetExtension()
    {
        var extensions = serialization.FileExtensions.ToArray();

        extensions.ShouldContain(".parquet");
        extensions.Length.ShouldBe(1);
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldReadCountriesFromParquetFile()
    {
        var countries = await fixture.DatabaseContexts["PARQUET"]
            .GetCountriesAsync(CancellationToken.None)
            .ToArrayAsync();

        countries.Length.ShouldBe(5);
        countries.Select(c => c.ID).ShouldContain(1100746772);
        countries.Select(c => c.Name).ShouldContain("United States");
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldReadCitiesFromParquetFile()
    {
        var cities = await fixture.DatabaseContexts["PARQUET"]
            .GetCitiesAsync(CancellationToken.None)
            .ToArrayAsync();

        cities.Length.ShouldBe(6);
        cities.ShouldContain(c => c.Name == "New York City");
        cities.ShouldContain(c => c.Name == "Vienna");
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldHandleBinaryStreamCorrectly()
    {
        var countries = await fixture.DatabaseContexts["PARQUET"]
            .GetCountriesAsync(CancellationToken.None)
            .ToArrayAsync();

        countries.Length.ShouldBeGreaterThan(0);

        foreach (var country in countries)
        {
            country.ID.ShouldBeGreaterThan(0);
            country.Name.ShouldNotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldRespectCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in fixture.DatabaseContexts["PARQUET"]
                .GetCountriesAsync(cts.Token))
            {
                // Should be cancelled before reading
            }
        });
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldHandleCaseInsensitivePropertyMatching()
    {
        var countries = await fixture.DatabaseContexts["PARQUET"]
            .GetCountriesAsync(CancellationToken.None)
            .ToArrayAsync();

        foreach (var country in countries)
        {
            country.ID.ShouldBeGreaterThan(0);
            country.Name.ShouldNotBeNullOrEmpty();
        }
    }
}
