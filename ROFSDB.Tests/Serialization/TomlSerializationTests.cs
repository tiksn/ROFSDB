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
public class TomlSerializationTests(DatabaseContextFixture fixture) : IClassFixture<DatabaseContextFixture>
{
    private readonly TomlSerialization serialization = new();
    private readonly DatabaseContextFixture fixture = fixture;

    [Fact]
    public void FileExtensions_ShouldReturnTomlExtensions()
    {
        var extensions = serialization.FileExtensions.ToArray();

        extensions.ShouldContain(".toml");
        extensions.Length.ShouldBe(1);
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldReadDocumentFromToml()
    {
        var toml = """
            id = 1419150635
            name = "Austria"
            """;

        var countries = await ReadCountriesAsync(toml);

        countries.Length.ShouldBe(1);
        countries[0].ID.ShouldBe(1419150635);
        countries[0].Name.ShouldBe("Austria");
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldMatchSnakeCaseProperties()
    {
        var toml = """
            id = 918909193
            name = "New York City"
            country_id = 1100746772
            """;

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(toml));
        var cities = await serialization.GetDocumentsAsync<City>(stream, CancellationToken.None).ToArrayAsync();

        cities.Length.ShouldBe(1);
        cities[0].ID.ShouldBe(918909193);
        cities[0].Name.ShouldBe("New York City");
        cities[0].CountryID.ShouldBe(1100746772);
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldRespectCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in serialization.GetDocumentsAsync<Country>(
                new MemoryStream(Encoding.UTF8.GetBytes("id = 1419150635\nname = 'Austria'")),
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
