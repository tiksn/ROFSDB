using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using TIKSN.ROFSDB.Tests.Fixtures;
using TIKSN.ROFSDB.Tests.Models;
using Xunit;
using Xunit.Abstractions;

namespace TIKSN.ROFSDB.Tests;

[Collection("Database Engine collection")]
public class DatabaseEngineTests(ITestOutputHelper testOutputHelper, DatabaseEngineFixture databaseEngineFixture)
{
    private readonly DatabaseEngineFixture databaseEngineFixture = databaseEngineFixture ?? throw new ArgumentNullException(nameof(databaseEngineFixture));
    private readonly ITestOutputHelper testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));

    [Theory]
    [InlineData("YAML")]
    [InlineData("TOML")]
    [InlineData("HCL")]
    [InlineData("PSD1")]
    public async Task CityCountTest(string fileFormat)
    {
        databaseEngineFixture.WriteFilesAndFoldersToTestOutput(fileFormat, testOutputHelper);

        var count = await databaseEngineFixture.DatabaseEngines[fileFormat].GetDocumentsAsync<City>("Cities", default).CountAsync(default);

        count.ShouldBe(6);
    }

    [Theory]
    [InlineData("YAML")]
    [InlineData("TOML")]
    [InlineData("HCL")]
    [InlineData("PSD1")]
    public async Task CollectionNameTest(string fileFormat)
    {
        databaseEngineFixture.WriteFilesAndFoldersToTestOutput(fileFormat, testOutputHelper);

        var actual = await databaseEngineFixture.DatabaseEngines[fileFormat].GetCollectionsAsync(default).ToArrayAsync(default);
        var expected = new[] { "Countries", "Cities" };

        actual.OrderBy(x => x).ShouldBe(expected.OrderBy(x => x));
    }

    [Theory]
    [InlineData("YAML")]
    [InlineData("TOML")]
    [InlineData("HCL")]
    [InlineData("PSD1")]
    public async Task CountryCityRelationsTest(string fileFormat)
    {
        databaseEngineFixture.WriteFilesAndFoldersToTestOutput(fileFormat, testOutputHelper);

        await foreach (var city in databaseEngineFixture.DatabaseEngines[fileFormat].GetDocumentsAsync<City>("Cities", default))
        {
            var countryFound = await databaseEngineFixture.DatabaseEngines[fileFormat].GetDocumentsAsync<Country>("Countries", default).AnyAsync(x => x.ID == city.CountryID);

            countryFound.ShouldBeTrue();
        }
    }

    [Theory]
    [InlineData("YAML")]
    [InlineData("TOML")]
    [InlineData("HCL")]
    [InlineData("PSD1")]
    public async Task CountryCountTest(string fileFormat)
    {
        databaseEngineFixture.WriteFilesAndFoldersToTestOutput(fileFormat, testOutputHelper);

        var count = await databaseEngineFixture.DatabaseEngines[fileFormat].GetDocumentsAsync<Country>("Countries", default).CountAsync(default);

        count.ShouldBe(5);
    }

    [Theory]
    [InlineData("YAML")]
    [InlineData("TOML")]
    [InlineData("HCL")]
    [InlineData("PSD1")]
    public async Task CountryIdTest(string fileFormat)
    {
        databaseEngineFixture.WriteFilesAndFoldersToTestOutput(fileFormat, testOutputHelper);

        var actual = await databaseEngineFixture.DatabaseEngines[fileFormat]
            .GetDocumentsAsync<Country>("Countries", default)
            .Select(x => x.ID)
            .ToArrayAsync(default);

        var expected = new[] { 1419150635, 965475701, 1552721979, 1501801186, 1100746772 };

        actual.OrderBy(x => x).ShouldBe(expected.OrderBy(x => x));
    }

    [Theory]
    [InlineData("YAML")]
    [InlineData("TOML")]
    [InlineData("HCL")]
    [InlineData("PSD1")]
    public async Task CountryNameTest(string fileFormat)
    {
        databaseEngineFixture.WriteFilesAndFoldersToTestOutput(fileFormat, testOutputHelper);

        var actual = await databaseEngineFixture.DatabaseEngines[fileFormat]
            .GetDocumentsAsync<Country>("Countries", default)
            .Select(x => x.Name)
            .ToArrayAsync(default);

        var expected = new[] { "Austria", "Canada", "France", "Italy", "United States" };

        actual.OrderBy(x => x).ShouldBe(expected.OrderBy(x => x));
    }
}