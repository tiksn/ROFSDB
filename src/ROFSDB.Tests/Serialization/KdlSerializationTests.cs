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
public class KdlSerializationTests(DatabaseContextFixture fixture) : IClassFixture<DatabaseContextFixture>
{
    private readonly DatabaseContextFixture fixture = fixture;
    private readonly KdlSerialization serialization = new();

    [Fact]
    public void FileExtensions_ShouldReturnKdlExtension()
    {
        var extensions = serialization.FileExtensions.ToArray();

        extensions.ShouldContain(".kdl");
        extensions.Length.ShouldBe(1);
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldReadCountriesFromKdlFile()
    {
        var countries = await fixture.DatabaseContexts["KDL"]
            .GetCountriesAsync(CancellationToken.None)
            .ToArrayAsync();

        countries.Length.ShouldBe(5);
        countries.Select(c => c.ID).ShouldContain(1100746772);
        countries.Select(c => c.Name).ShouldContain("United States");
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldReadCitiesFromKdlFile()
    {
        var cities = await fixture.DatabaseContexts["KDL"]
            .GetCitiesAsync(CancellationToken.None)
            .ToArrayAsync();

        cities.Length.ShouldBe(6);
        cities.ShouldContain(c => c.Name == "New York City");
        cities.ShouldContain(c => c.Name == "Vienna");
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldReadMultipleNodesFromOneFile()
    {
        var countries = await ReadCountriesAsync("""
            country id=1 name="Austria"
            country id=2 name="Canada"
            """);

        countries.Select(x => x.Name).ShouldBe(["Austria", "Canada"]);
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldReadInlineProperties()
    {
        var countries = await ReadCountriesAsync("country id=1419150635 name=\"Austria\"");

        countries.Single().ID.ShouldBe(1419150635);
        countries.Single().Name.ShouldBe("Austria");
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldReadChildScalarNodes()
    {
        var countries = await ReadCountriesAsync("""
            country {
              id 1419150635
              name "Austria"
            }
            """);

        countries.Single().ID.ShouldBe(1419150635);
        countries.Single().Name.ShouldBe("Austria");
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldThrowForTypeNodeMismatch()
    {
        await Should.ThrowAsync<FormatException>(async () =>
        {
            _ = await ReadCountriesAsync("city id=1 name=\"Austin\" country-id=2");
        });
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldThrowForUnknownMember()
    {
        await Should.ThrowAsync<FormatException>(async () =>
        {
            _ = await ReadCountriesAsync("country id=1 name=\"Austria\" unknown=\"value\"");
        });
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldThrowForDuplicateMember()
    {
        await Should.ThrowAsync<FormatException>(async () =>
        {
            _ = await ReadCountriesAsync("""
                country id=1 {
                  id 2
                  name "Austria"
                }
                """);
        });
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldMatchMembersCaseSensitively()
    {
        await Should.ThrowAsync<FormatException>(async () =>
        {
            _ = await ReadCountriesAsync("country ID=1 name=\"Austria\"");
        });
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldUseDefaultsForMissingMembers()
    {
        var countries = await ReadCountriesAsync("country id=1419150635");

        countries.Single().ID.ShouldBe(1419150635);
        countries.Single().Name.ShouldBeNull();
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldRespectCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            _ = await ReadCountriesAsync("country id=1419150635 name=\"Austria\"", cts.Token);
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
