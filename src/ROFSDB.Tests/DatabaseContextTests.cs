using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using TIKSN.ROFSDB.Tests.Fixtures;
using TIKSN.ROFSDB.Tests.Models;
using Xunit;
using Xunit.Abstractions;

namespace TIKSN.ROFSDB.Tests;

[Collection("Database Context collection")]
public class DatabaseContextTests(ITestOutputHelper testOutputHelper, DatabaseContextFixture databaseContextFixture)
{
    private readonly DatabaseContextFixture databaseContextFixture = databaseContextFixture ?? throw new ArgumentNullException(nameof(databaseContextFixture));
    private readonly ITestOutputHelper testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));

    [Theory]
    [InlineData("YAML")]
    [InlineData("TOML")]
    [InlineData("HCL")]
    [InlineData("JSON")]
    [InlineData("PSD1")]
    [InlineData("PARQUET")]
    public async Task CityCountTest(string fileFormat)
    {
        databaseContextFixture.WriteFilesAndFoldersToTestOutput(fileFormat, testOutputHelper);

        var count = await databaseContextFixture.DatabaseContexts[fileFormat].GetCitiesAsync(default).CountAsync(default);

        count.ShouldBe(6);
    }

    [Theory]
    [InlineData("YAML")]
    [InlineData("TOML")]
    [InlineData("HCL")]
    [InlineData("JSON")]
    [InlineData("PSD1")]
    [InlineData("PARQUET")]
    public async Task CountryCityRelationsTest(string fileFormat)
    {
        databaseContextFixture.WriteFilesAndFoldersToTestOutput(fileFormat, testOutputHelper);

        await foreach (var city in databaseContextFixture.DatabaseContexts[fileFormat].GetCitiesAsync(default))
        {
            var countryFound = await databaseContextFixture.DatabaseContexts[fileFormat].GetCountriesAsync(default).AnyAsync(x => x.ID == city.CountryID);

            countryFound.ShouldBeTrue();
        }
    }

    [Theory]
    [InlineData("YAML")]
    [InlineData("TOML")]
    [InlineData("HCL")]
    [InlineData("JSON")]
    [InlineData("PSD1")]
    [InlineData("PARQUET")]
    public async Task CountryCountTest(string fileFormat)
    {
        databaseContextFixture.WriteFilesAndFoldersToTestOutput(fileFormat, testOutputHelper);

        var count = await databaseContextFixture.DatabaseContexts[fileFormat].GetCountriesAsync(default).CountAsync(default);

        count.ShouldBe(5);
    }

    [Theory]
    [InlineData("YAML")]
    [InlineData("TOML")]
    [InlineData("HCL")]
    [InlineData("JSON")]
    [InlineData("PSD1")]
    [InlineData("PARQUET")]
    public async Task CountryIdTest(string fileFormat)
    {
        databaseContextFixture.WriteFilesAndFoldersToTestOutput(fileFormat, testOutputHelper);

        var actual = await databaseContextFixture.DatabaseContexts[fileFormat]
            .GetCountriesAsync(default)
            .Select(x => x.ID)
            .ToArrayAsync(default);

        var expected = new[] { 1419150635, 965475701, 1552721979, 1501801186, 1100746772 };

        actual.OrderBy(x => x).ShouldBe(expected.OrderBy(x => x));
    }

    [Theory]
    [InlineData("YAML")]
    [InlineData("TOML")]
    [InlineData("HCL")]
    [InlineData("JSON")]
    [InlineData("PSD1")]
    [InlineData("PARQUET")]
    public async Task CountryNameTest(string fileFormat)
    {
        databaseContextFixture.WriteFilesAndFoldersToTestOutput(fileFormat, testOutputHelper);

        var actual = await databaseContextFixture.DatabaseContexts[fileFormat]
            .GetCountriesAsync(default)
            .Select(x => x.Name)
            .ToArrayAsync(default);

        var expected = new[] { "Austria", "Canada", "France", "Italy", "United States" };

        actual.OrderBy(x => x).ShouldBe(expected.OrderBy(x => x));
    }
}
